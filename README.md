# Progress

Checklist on roadmap

## Clean Architecture
- [x] `CQRS`
    - [X] Commands with `PostgreSql` + `EF Core`
    - [X] Queries with `MongoDB` + `EF Core`
    - [ ] CDC using Debezium
- [x] `MediatR`

## Vertical Slice Architecture
> Replace with Vertical Slice Architecture

## Microservice
- [x] ApacheKafka
    - [x] Producer, producer batch
    - [x] Consumer, consumer batch
    - [x] Generate kafka topic, configuration using code

- [x] RabbitMq
    - [ ] Publish, Subscribe message
    - [ ] Fanout Pattern
    - [ ] Implement kafka pattern into rabbitmq

- [x] Krakend (API Gateway)
    - [ ] Configuration json krakend
    - [ ] Optimise
    - [ ] Register into Aspire

- [x] Dapr
    - [ ] Configuration for switch kafka and rabbitmq pubsub easy
    - [ ] State management

## Orchestration

- [x] Aspire
    - [x] RDBMS - `PostgreSQL`
    - [ ] NoSQL - `MongoDB`
    - [x] Distributed Cache - `Redis`
    - [x] Aspire Dashboard
    - [x] Apache Kafka with KafkaUI
    - [x] RabbitMQ
    - [x] Krakend with dashboard
    - [ ] ...

## Health Checks

- [x] Self
- [x] PostgreSQL Database
- [x] Redis
- [ ] MongoDB

## Observability - `Aspire Dashboard`

- [x] Logging
- [x] Metrics
- [x] Distributed Tracing

## DAPR

- [x] Pub / Sub - `Redis Streams`
    - [x] Code Tour
- [ ] Service Invocation
- [x] State Management - `Redis`
    - [x] Save state
    - [x] Read state
    - [x] Delete state
- [x] Secrets Management - Json File Based
- [x] Configurations Store
- [x] Bindings
    - [x] Input: `Cron`
        - [x] Code Tour

## Misc

- [x] Git ignore
- [x] Editor config
- [x] Build props
- [x] Code Analysis
- [x] Tool manifest
- [ ] Automapper / Mapster
- [x] Validation `Fluent Validation`
- [x] `Dependabot` to update outdated nuget packages
- [x] Security Score Card

## Build

- [x] Local build - Notebook
- [x] GitHub Action
    - [x] Tool Restore
    - [x] Package Restore
    - [x] Build
    - [x] Test
    - [x] Publish Coverage Report
    - [x] Pack as nuget package
    - [x] Push to nuget.org

## Deployment

- [x] Install Dependencies
    - [x] Dapr
    - [x] Redis
    - [x] Postgresql
    - [x] Aspire Dashboard
- [x] Dapr Components
    - [x] State store
    - [x] Pub / Sub
    - [x] Secret store
    - [x] Configuration store
    - [x] Cron Binding
- [x] Deploy API

## Performance

- [ ] Load Testing - using `K6`
