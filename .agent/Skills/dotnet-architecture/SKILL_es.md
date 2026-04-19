---
name: dotnet-architecture
description: Diseña o revisa la arquitectura de soluciones .NET abarcando monolitos modulares, clean architecture, vertical slices, microservicios, DDD, CQRS y límites cloud-native sin sobre-ingeniería.
license: Private
compatibility: Requiere .NET 8+ SDK. Óptimo cuando la estructura del proyecto o los límites de servicio están en alcance.
---

# Arquitectura .NET

## Cuándo usar esta skill

- Al elegir la arquitectura de un sistema .NET nuevo o en evolución.
- Al revisar los límites de capas, fronteras del dominio o descomposición de servicios.
- Al decidir si el uso de clean architecture, vertical slices, CQRS o microservicios está justificado.

## Flujo de trabajo

1. **Empieza por los límites funcionales de negocio y la frecuencia de cambio**, no por tu diagrama arquitectónico preferido.
2. Utiliza patrones simples de **monolito modular** por defecto. Muévete hacia microservicios solo cuando la necesidad de equipos autónomos, el requerimiento de alta escalabilidad o la segmentación de despliegues justifiquen el coste operacional añadido.
3. Aplica **DDD y CQRS** únicamente allí donde las reglas de negocio sean genuinamente complejas; evita forzar _aggregates_ o _command pipelines_ en dominios de puro CRUD sin retorno de inversión.
4. Mantén las dependencias orientadas hacia el centro (inward) si aplicas **clean architecture**, pero evita crear proyectos adicionales que solo añaden burocracia técnica sin clarificar quién posee qué.
5. **Haz explícita la integración:** usa contratos, define la propiedad del almacenamiento, la mensajería, el modelo de consistencia empleado y las expectativas funcionales y de observabilidad.
6. Utiliza `dotnet-aspire` cuando la orquestación en local, el Service Discovery y la observabilidad del entorno de desarrollo formen parte de la estrategia arquitectónica.

## Entregables

- Una dirección arquitectónica alineada con la complejidad del sistema.
- Límites claros para los proyectos y sus dependencias.
- Notas sobre estrategias de migración o concesiones e inconvenientes al alterar estructuras preexistentes.

## Validación

- La base propuesta **mitiga, no exacerba, la complejidad accidental**.
- La propiedad de los datos y los flujos de integración están estrictamente definidos.
- La arquitectura facilita tanto su testeo como su operación real, evadiendo la trampa de un "diseño perfecto solo en papel".

## Referencias

- [references/patterns.md](references/patterns.md) - Implementaciones en detalle de Patrones Arquitectónicos (Clean Architecture, Vertical Slices, DDD, CQRS, Modular Monolith y Microservicios) apalancados sobre ejemplos en C# 12+.
- [references/anti-patterns.md](references/anti-patterns.md) - Errores estructurales comunes, enfatizando aspectos como la sobre-abstracción, dominios anémicos, microservicios prematuros y falacias del *cargo cult*.
