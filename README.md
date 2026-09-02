# flujos-aprobacion
Sistema (test) de flujos de aprobación, utilizando minimal APIs

**Descripción:** Sistema de flujos de aprobación  
**Características:**
- Los flujos se deben crear a partir de **Plantillas**
- Cada paso dentro de un flujo, tiene un autorizador _pre-asignado_
- Cada paso tiene un orden específico
- Todos los flujos son secuenciales
- Si se rechaza una etapa, se rechaza todo el flujo
- Solamente el autorizador o el adminitrador pueden aprobar 
- Para tomar una decisión, se manda si es **APROBADO** o **RECHAZADO** y una _observación_
- Utilizar **Swagger** o **Scalar** para tener una interfaz con la documentación de la API (basado en la especificación **OpenAPI**)
