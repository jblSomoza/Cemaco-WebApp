# Presentaciones Cemaco

- `Cemaco_Exposicion_3min.pptx`: 3 diapositivas; empresa, problema y objetivos. Guion estimado: 2 min 55 s.
- `Cemaco_Respaldo_Preguntas.pptx`: 19 diapositivas de apoyo para preguntas prácticas y analíticas de los 12 minutos siguientes.

Las capturas de inicio, seguimiento, administración, página 404 y error de conexión se tomaron de la aplicación en ejecución el 29 de octubre de 2026. Las capturas de catálogo, carrito, confirmación y reporte se obtuvieron renderizando las vistas reales con datos de demostración; no representan una transacción confirmada en SQL Server. En este entorno aislado la autenticación integrada falla con SSPI.

Antes de la exposición, abran la aplicación desde una cuenta Windows que tenga acceso a `.\SQLEXPRESS`, ejecuten el script SQL y comprueben una compra completa. Si obtienen capturas con datos reales, pueden sustituir las imágenes de demostración y regenerar las presentaciones.

Las diapositivas PPTX son imágenes de 16:9. El contenido editable está en `generar_fuentes.mjs`; los HTML generados quedan en `fuentes/` y las capturas de cada lámina en `rendered/`. `render_slides.mjs` usa Edge en modo de depuración remota en el puerto 9240; `empaquetar.ps1` crea los dos PPTX a partir de las imágenes renderizadas.
