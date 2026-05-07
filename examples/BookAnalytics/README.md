# BookAnalytics Example

This example demonstrates aggregate analytics retrieval through `IBookAnalyticsClient`:

- `GetSummaryAsync(...)`

It maps request values to:

- `GET /book-analytics/summary?bookId=...&from=...&to=...`

Notes:

- `Granularity` and `IncludeUniqueReaders` are currently future-facing fields in the SDK model and are not sent to the current backend summary endpoint.

Run:

- `examples/BookAnalytics/BookAnalyticsExample.csx`
