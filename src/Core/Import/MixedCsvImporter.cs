using System.Globalization;
using System.Text;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<ImportEntityDto> Load(string path)
    {
        var items = new List<ImportEntityDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith('#'))
            {
                continue;
            }

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add(
                        $"рядок {number}: {failed.Reason}"
                    );
                    break;
            }
        }

        return new ImportResult<ImportEntityDto>(
            items,
            errors
        );
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries
        );

        return parts switch
        {
            ["P", _, "", _, _, _]
                or ["P", _, _, "", _, _]
                => new ParseFailed(
                    "у товару SKU або назва порожні"
                ),

            ["P", _, _, _, "", _]
                => new ParseFailed(
                    "у товару одиниця виміру порожня"
                ),

            ["P", _, _, _, _, var qty]
                when !int.TryParse(
                    qty,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int q
                ) || q < 0
                => new ParseFailed(
                    $"кількість '{qty}' некоректна"
                ),

            [
                "P",
                var id,
                var sku,
                var name,
                var unit,
                var qty
            ]
                => new ParseOk(
                    new ProductDto(
                        id,
                        sku,
                        name,
                        unit,
                        int.Parse(
                            qty,
                            CultureInfo.InvariantCulture
                        )
                    )
                ),

            ["W", "", _, _]
                or ["W", _, "", _]
                or ["W", _, _, ""]
                => new ParseFailed(
                    "дані складу не можуть бути порожніми"
                ),

            ["W", var id, var name, var city]
                => new ParseOk(
                    new WarehouseDto(
                        id,
                        name,
                        city
                    )
                ),

            ["P", ..]
                => new ParseFailed(
                    $"для товару P очікую 6 колонок, отримав {parts.Length}"
                ),

            ["W", ..]
                => new ParseFailed(
                    $"для складу W очікую 4 колонки, отримав {parts.Length}"
                ),

            [var prefix, ..]
                => new ParseFailed(
                    $"невідомий префікс типу '{prefix}'"
                ),

            _ => new ParseFailed(
                "невідомий формат рядка"
            )
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(ImportEntityDto Value)
        : ParseOutcome;

    private sealed record ParseFailed(string Reason)
        : ParseOutcome;
}