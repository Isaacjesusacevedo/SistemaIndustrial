# Sistema anterior (Excel)

Esta carpeta contiene la documentación del **sistema original en Excel** que
se usó antes de desarrollar la aplicación web.

## Contexto

- **2018-2020**: Control manual en papel
- **2020-2022**: Migración a Excel (este sistema)
- **2023-hoy**: Aplicación web (reemplazo del Excel)

El Excel fue la base para diseñar las funcionalidades de la app. Todas las
columnas, cálculos y flujos de la app vienen de ese archivo.

## Archivos

| Archivo | Descripción | Estado |
|---|---|---|
| `stock-semanal-sample.csv` | Captura del formato semanal (una hoja por semana), **limpia** | ⏳ Pendiente de subir |

> Antes de subir el CSV: quitar las celdas con bromas internas
> (revisar filas con texto no profesional) y las filas vacías al final.
> Un reclutador no debería ver ese contenido.

## Estructura del Excel

| Columna | Equivalente en la app |
|---|---|
| Productos | `TiradoProducto.Nombre` |
| PEDIDO SEMANAL | `TiradoProducto.PedidoSemanal` |
| STOCK INICIAL | `TiradoProducto.StockInicial` |
| SÁBADO–VIERNES | `Tirada.Cantidad` (por día) |
| STOCK TOTAL | Calculado: `StockInicial + Tirado − Pedido` |
| TACHOS ESTIMADOS | Calculado: `StockTotal / PromedioPorTacho` |
| "FALTAN: -X" | Subtotal por grupo de productos (solo Excel, la app no lo tiene) |

## Limitaciones del Excel

| Limitación | Cómo lo resuelve la app |
|---|---|
| Una hoja por semana | Base de datos centralizada |
| Fórmulas se rompían | Cálculos automáticos en backend |
| Sin validación | Validación en formularios |
| Compartir el archivo | Acceso web concurrente |
| No hay histórico | Todas las semanas en BD |
| Errores de tipeo | Validación + dropdowns |
| Sin autenticación | JWT + roles |

## Resultados medidos

| Métrica | Excel | App |
|---|---|---|
| Control de stock semanal | 3 horas | **1 hora** |
| Errores técnicos | Frecuentes | **−60%** |
| Productividad de sectores | Base | **+40%** |
| Errores de cálculo | Ocurrían | **0** |

## Diferencias de nombres conocidas

Entre el Excel original y el catálogo cargado hay diferencias pendientes
de confirmar (cuál es el nombre correcto):

| Excel original | En el sistema |
|---|---|
| Citro Atomic | Cito Atomic |
| Banana | Bariana |
| Élite | Elite |
| Ntp | Nitp |
| Inferno | Intenso |
| Cwb | Cvb |
| Ctrl z | Citriz |
| Ilussion | Illusion |
| Aluwash | Alu wash |
