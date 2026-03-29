var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/", () => Results.Content(GatewayDocs.Html, "text/html"));

app.MapReverseProxy();

app.Run();

static class GatewayDocs
{
    public const string Html = """
    <!DOCTYPE html>
    <html lang="es">
    <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <title>TuColmadoRD — API Gateway</title>
        <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
        <style>
            *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
            :root {
                --bg: #0a0e1a;
                --surface: #111827;
                --border: #1f2937;
                --accent: #6366f1;
                --accent2: #8b5cf6;
                --green: #10b981;
                --blue: #3b82f6;
                --orange: #f59e0b;
                --red: #ef4444;
                --text: #f1f5f9;
                --muted: #64748b;
            }
            body { font-family: 'Inter', sans-serif; background: var(--bg); color: var(--text); min-height: 100vh; }
            header {
                background: linear-gradient(135deg, #1e1b4b 0%, #0a0e1a 60%);
                border-bottom: 1px solid var(--border);
                padding: 2rem;
                text-align: center;
                position: relative;
                overflow: hidden;
            }
            header::before {
                content: '';
                position: absolute; inset: 0;
                background: radial-gradient(ellipse at 50% -20%, rgba(99,102,241,0.3) 0%, transparent 70%);
                pointer-events: none;
            }
            .badge { display: inline-block; background: rgba(99,102,241,0.2); border: 1px solid rgba(99,102,241,0.4); border-radius: 9999px; padding: 0.25rem 0.75rem; font-size: 0.75rem; font-weight: 600; color: #a5b4fc; letter-spacing: 0.05em; margin-bottom: 1rem; }
            h1 { font-size: 2.5rem; font-weight: 700; background: linear-gradient(135deg, #fff 30%, #a5b4fc); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text; }
            .subtitle { color: var(--muted); margin-top: 0.5rem; font-size: 1rem; }
            main { max-width: 900px; margin: 2.5rem auto; padding: 0 1.5rem 4rem; }
            .section-title { font-size: 0.7rem; font-weight: 600; letter-spacing: 0.1em; color: var(--muted); text-transform: uppercase; margin-bottom: 1rem; margin-top: 2.5rem; }
            .card {
                background: var(--surface);
                border: 1px solid var(--border);
                border-radius: 12px;
                overflow: hidden;
                margin-bottom: 0.75rem;
                transition: border-color 0.2s;
            }
            .card:hover { border-color: var(--accent); }
            .endpoint {
                display: flex; align-items: flex-start; gap: 1rem;
                padding: 1rem 1.25rem;
            }
            .method {
                font-size: 0.7rem; font-weight: 700; letter-spacing: 0.05em;
                border-radius: 6px; padding: 0.25rem 0.6rem;
                min-width: 56px; text-align: center; flex-shrink: 0; margin-top: 2px;
            }
            .GET    { background: rgba(16,185,129,0.15); color: var(--green); border: 1px solid rgba(16,185,129,0.3); }
            .POST   { background: rgba(59,130,246,0.15); color: var(--blue); border: 1px solid rgba(59,130,246,0.3); }
            .PUT    { background: rgba(245,158,11,0.15); color: var(--orange); border: 1px solid rgba(245,158,11,0.3); }
            .DELETE { background: rgba(239,68,68,0.15); color: var(--red); border: 1px solid rgba(239,68,68,0.3); }
            .ep-info { flex: 1; }
            .ep-path { font-family: 'Courier New', monospace; font-size: 0.9rem; color: var(--text); font-weight: 500; }
            .ep-desc { font-size: 0.8rem; color: var(--muted); margin-top: 0.25rem; }
            .tag { display: inline-block; font-size: 0.65rem; background: rgba(99,102,241,0.15); color: #a5b4fc; border: 1px solid rgba(99,102,241,0.3); border-radius: 4px; padding: 0.1rem 0.4rem; margin-top: 0.4rem; }
            .status-bar { display: flex; gap: 1.5rem; justify-content: center; margin-top: 2rem; flex-wrap: wrap; }
            .status-item { display: flex; align-items: center; gap: 0.5rem; font-size: 0.8rem; color: var(--muted); }
            .dot { width: 8px; height: 8px; border-radius: 50%; background: var(--green); animation: pulse 2s infinite; }
            @keyframes pulse { 0%,100% { opacity:1; } 50% { opacity:0.4; } }
            footer { text-align: center; padding: 2rem; color: var(--muted); font-size: 0.75rem; border-top: 1px solid var(--border); }
        </style>
    </head>
    <body>
        <header>
            <div class="badge">API Gateway v1.0</div>
            <h1>TuColmadoRD API</h1>
            <p class="subtitle">Punto de entrada unificado para todos los microservicios</p>
            <div class="status-bar">
                <div class="status-item"><span class="dot"></span>Gateway · YARP</div>
                <div class="status-item"><span class="dot"></span>API .NET 10</div>
                <div class="status-item"><span class="dot"></span>Auth Node.js</div>
            </div>
        </header>
        <main>
            <div class="section-title">Autenticación</div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/auth/register</div><div class="ep-desc">Registrar un nuevo usuario en el sistema</div><span class="tag">Auth Service</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/auth/login</div><div class="ep-desc">Iniciar sesión y obtener token JWT</div><span class="tag">Auth Service</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/auth/refresh</div><div class="ep-desc">Refrescar el token de acceso</div><span class="tag">Auth Service</span></div></div></div>

            <div class="section-title">Inventario</div>
            <div class="card"><div class="endpoint"><span class="method GET">GET</span><div class="ep-info"><div class="ep-path">/api/products</div><div class="ep-desc">Listar todos los productos del colmado</div><span class="tag">Core API</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/products</div><div class="ep-desc">Agregar un nuevo producto</div><span class="tag">Core API</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method GET">GET</span><div class="ep-info"><div class="ep-path">/api/categories</div><div class="ep-desc">Listar categorías de productos</div><span class="tag">Core API</span></div></div></div>

            <div class="section-title">Ventas</div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/sales</div><div class="ep-desc">Registrar una nueva venta</div><span class="tag">Core API</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method GET">GET</span><div class="ep-info"><div class="ep-path">/api/sales/{id}</div><div class="ep-desc">Obtener detalle de una venta</div><span class="tag">Core API</span></div></div></div>

            <div class="section-title">Clientes</div>
            <div class="card"><div class="endpoint"><span class="method GET">GET</span><div class="ep-info"><div class="ep-path">/api/customers</div><div class="ep-desc">Listar clientes registrados</div><span class="tag">Core API</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/customers</div><div class="ep-desc">Registrar un nuevo cliente</div><span class="tag">Core API</span></div></div></div>

            <div class="section-title">Tesorería</div>
            <div class="card"><div class="endpoint"><span class="method GET">GET</span><div class="ep-info"><div class="ep-path">/api/cashbox</div><div class="ep-desc">Consultar estado de la caja registradora</div><span class="tag">Core API</span></div></div></div>
            <div class="card"><div class="endpoint"><span class="method POST">POST</span><div class="ep-info"><div class="ep-path">/api/expenses</div><div class="ep-desc">Registrar un gasto</div><span class="tag">Core API</span></div></div></div>
        </main>
        <footer>TuColmadoRD &copy; 2026 — Francisco Daniel Castro Borrome</footer>
    </body>
    </html>
    """;
}
