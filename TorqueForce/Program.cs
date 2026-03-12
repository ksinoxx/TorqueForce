using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;



var options = new WebApplicationOptions
{
    
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "public"),
    ContentRootPath = Directory.GetCurrentDirectory()
};

var builder = WebApplication.CreateBuilder(options);

var cs = builder.Configuration.GetConnectionString("Default")
         ?? throw new Exception("Нет ConnectionStrings:Default в appsettings.json");

var app = builder.Build();


app.UseDefaultFiles();


app.UseStaticFiles();

static string NormalizeStatus(string s) => (s ?? "").Trim();

static bool IsAllowedStatus(string s)
{
    s = NormalizeStatus(s);
    return s == "Принят" || s == "В обработке" || s == "Выдан";
}


using (var con = new SqlConnection(cs))
{
    try
    {
        await con.OpenAsync();
        await con.ExecuteAsync("""
            IF NOT EXISTS (SELECT 1 FROM [step] WHERE name_step = N'Принят')
                INSERT INTO [step](name_step) VALUES (N'Принят');

            IF NOT EXISTS (SELECT 1 FROM [step] WHERE name_step = N'В обработке')
                INSERT INTO [step](name_step) VALUES (N'В обработке');

            IF NOT EXISTS (SELECT 1 FROM [step] WHERE name_step = N'Выдан')
                INSERT INTO [step](name_step) VALUES (N'Выдан');
        """);
    }
    catch (Microsoft.Data.SqlClient.SqlException ex)
    {
        Console.Error.WriteLine($"Не удалось подключиться к базе данных при старте: {ex.Message}");
        
    }
}


app.MapGet("/api/parts", async () =>
{
    using var con = new SqlConnection(cs);

    const string sql = @"
SELECT
  part_id   AS PartId,
  part_name AS PartName,
  price     AS Price,
  stock     AS Stock
FROM part
ORDER BY part_name;
";

    var parts = await con.QueryAsync<PartDto>(sql);
    return Results.Ok(parts);
});


app.MapPost("/api/orders", async (CreateOrderRequest req) =>
{
    if (req is null) return Results.BadRequest("Пустой запрос");
    if (req.Client is null) return Results.BadRequest("client обязателен");
    if (string.IsNullOrWhiteSpace(req.Client.Name) || string.IsNullOrWhiteSpace(req.Client.Phone))
        return Results.BadRequest("Заполните имя и телефон");
    if (req.Items is null || req.Items.Count == 0)
        return Results.BadRequest("items пустые");

    
    var items = req.Items
        .Where(x => x.PartId > 0 && x.Quantity > 0)
        .GroupBy(x => x.PartId)
        .Select(g => new OrderItemDto(g.Key, g.Sum(x => x.Quantity)))
        .ToList();

    if (items.Count == 0) return Results.BadRequest("Нет корректных items");

    await using var con = new SqlConnection(cs);
    await con.OpenAsync();

    await using var tx = con.BeginTransaction();

    try
    {
        
        var ids = items.Select(i => i.PartId).Distinct().ToArray();

        var dbParts = (await con.QueryAsync<PartDto>(
            """
            SELECT part_id AS PartId, part_name AS PartName, price AS Price, stock AS Stock
            FROM part
            WHERE part_id IN @ids;
            """,
            new { ids }, tx)).ToList();

        if (dbParts.Count != ids.Length)
            return Results.BadRequest("Некоторые partId не найдены");

        var partById = dbParts.ToDictionary(p => p.PartId, p => p);

        
        foreach (var it in items)
        {
            var p = partById[it.PartId];
            if (p.Stock < it.Quantity)
                return Results.BadRequest($"Недостаточно на складе: {p.PartName}");
        }

        
        var clientId = await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO client(name_client, phone, email)
            VALUES (@name, @phone, COALESCE(@email, ''));
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            new { name = req.Client.Name.Trim(), phone = req.Client.Phone.Trim(), email = req.Client.Email }, tx);

        
        decimal total = 0;
        foreach (var it in items)
        {
            var p = partById[it.PartId];
            total += p.Price * it.Quantity;
        }

        
        var orderId = await con.ExecuteScalarAsync<int>(
            """
            INSERT INTO customer_order(client_id, employee_id, order_date, total_price)
            VALUES (@clientId, NULL, GETDATE(), @total);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            new { clientId, total }, tx);

        
        foreach (var it in items)
        {
            var p = partById[it.PartId];

            
            var affected = await con.ExecuteAsync(
                """
                UPDATE part
                SET stock = stock - @qty
                WHERE part_id = @id AND stock >= @qty;
                """,
                new { id = it.PartId, qty = it.Quantity }, tx);

            if (affected == 0)
                return Results.BadRequest($"Склад изменился, не хватает товара: {p.PartName}");

            await con.ExecuteAsync(
                """
                INSERT INTO order_part(customer_order_id, part_id, quantity, price)
                VALUES (@orderId, @partId, @qty, @price);
                """,
                new { orderId, partId = it.PartId, qty = it.Quantity, price = p.Price }, tx);
        }

        
        var stepId = await con.ExecuteScalarAsync<int>(
            "SELECT step_id FROM [step] WHERE name_step = N'Принят';",
            transaction: tx);

        await con.ExecuteAsync(
            """
            INSERT INTO order_step(customer_order_id, step_id, date_start, date_end)
            VALUES (@orderId, @stepId, GETDATE(), NULL);
            """,
            new { orderId, stepId }, tx);

        await tx.CommitAsync();

        return Results.Ok(new { orderId });
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }
});


app.MapGet("/api/admin/orders", async () =>
{
    using var con = new SqlConnection(cs);

    const string sql = """
        SELECT
            co.customer_order_id AS OrderId,
            co.order_date        AS OrderDate,
            co.total_price       AS TotalPrice,
            c.name_client        AS ClientName,
            ISNULL(s.name_step, N'—') AS Status
        FROM customer_order co
        JOIN client c ON c.client_id = co.client_id
        OUTER APPLY (
            SELECT TOP(1) os.step_id
            FROM order_step os
            WHERE os.customer_order_id = co.customer_order_id
            ORDER BY os.date_start DESC
        ) lastos
        LEFT JOIN [step] s ON s.step_id = lastos.step_id
        ORDER BY co.customer_order_id DESC;
    """;

    var rows = await con.QueryAsync<AdminOrderRowDto>(sql);
    return Results.Ok(rows);
});


app.MapGet("/api/admin/orders/{id:int}", async (int id) =>
{
    using var con = new SqlConnection(cs);

    var header = await con.QuerySingleOrDefaultAsync(
        """
        SELECT
            co.customer_order_id AS OrderId,
            co.order_date        AS OrderDate,
            co.total_price       AS TotalPrice,
            c.name_client        AS ClientName,
            ISNULL(s.name_step, N'—') AS Status
        FROM customer_order co
        JOIN client c ON c.client_id = co.client_id
        OUTER APPLY (
            SELECT TOP(1) os.step_id
            FROM order_step os
            WHERE os.customer_order_id = co.customer_order_id
            ORDER BY os.date_start DESC
        ) lastos
        LEFT JOIN [step] s ON s.step_id = lastos.step_id
        WHERE co.customer_order_id = @id;
        """,
        new { id });

    if (header is null) return Results.NotFound("Заказ не найден");

    var items = (await con.QueryAsync<AdminOrderItemDto>(
        """
        SELECT
            op.part_id   AS PartId,
            p.part_name  AS PartName,
            op.price     AS Price,
            op.quantity  AS Quantity
        FROM order_part op
        JOIN part p ON p.part_id = op.part_id
        WHERE op.customer_order_id = @id
        ORDER BY op.order_part_id;
        """,
        new { id })).ToList();

    var history = (await con.QueryAsync<AdminOrderHistoryDto>(
        """
        SELECT
            s.name_step AS Status,
            os.date_start AS DateStart,
            os.date_end   AS DateEnd
        FROM order_step os
        JOIN [step] s ON s.step_id = os.step_id
        WHERE os.customer_order_id = @id
        ORDER BY os.date_start;
        """,
        new { id })).ToList();

    var dto = new AdminOrderDetailsDto(
        (int)header.OrderId, (DateTime)header.OrderDate, (decimal)header.TotalPrice,
        (string)header.ClientName, (string)header.Status,
        items, history);

    return Results.Ok(dto);
});


app.MapPost("/api/admin/orders/{id:int}/status", async (int id, StatusRequest req) =>
{
    var newStatus = NormalizeStatus(req?.Status ?? "");
    if (!IsAllowedStatus(newStatus))
        return Results.BadRequest("Недопустимый статус");

    await using var con = new SqlConnection(cs);
    await con.OpenAsync();
    await using var tx = con.BeginTransaction();

    try
    {
        var exists = await con.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM customer_order WHERE customer_order_id = @id;",
            new { id }, tx);

        if (exists == 0) return Results.NotFound("Заказ не найден");

        var currentStatus = await con.ExecuteScalarAsync<string?>(
            """
            SELECT TOP(1) s.name_step
            FROM order_step os
            JOIN [step] s ON s.step_id = os.step_id
            WHERE os.customer_order_id = @id
            ORDER BY os.date_start DESC;
            """,
            new { id }, tx);

        if (NormalizeStatus(currentStatus ?? "") == newStatus)
        {
            await tx.CommitAsync();
            return Results.Ok(new { ok = true, status = newStatus });
        }

        var stepId = await con.ExecuteScalarAsync<int>(
            "SELECT step_id FROM [step] WHERE name_step = @name;",
            new { name = newStatus }, tx);

        
        await con.ExecuteAsync(
            "UPDATE order_step SET date_end = GETDATE() WHERE customer_order_id = @id AND date_end IS NULL;",
            new { id }, tx);

        
        await con.ExecuteAsync(
            """
            INSERT INTO order_step(customer_order_id, step_id, date_start, date_end)
            VALUES (@id, @stepId, GETDATE(), NULL);
            """,
            new { id, stepId }, tx);

        await tx.CommitAsync();
        return Results.Ok(new { ok = true, status = newStatus });
    }
    catch
    {
        await tx.RollbackAsync();
        throw;
    }
});

app.Run();


record PartDto(int PartId, string PartName, decimal Price, int Stock);

record ClientDto(string Name, string Phone, string? Email);
record OrderItemDto(int PartId, int Quantity);
record CreateOrderRequest(ClientDto Client, List<OrderItemDto> Items);

record AdminOrderRowDto(int OrderId, DateTime OrderDate, decimal TotalPrice, string ClientName, string Status);
record AdminOrderItemDto(int PartId, string PartName, decimal Price, int Quantity);
record AdminOrderHistoryDto(string Status, DateTime DateStart, DateTime? DateEnd);
record AdminOrderDetailsDto(
    int OrderId, DateTime OrderDate, decimal TotalPrice, string ClientName, string Status,
    List<AdminOrderItemDto> Items, List<AdminOrderHistoryDto> History);

record StatusRequest(string Status);