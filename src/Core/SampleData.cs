using Core.Domain;

namespace Core;

public static class SampleData
{
    public static IEnumerable<Product> Products() => new[]
    {
        Product.Create("P-001", "ARO-MILK-900", "Молоко ARO 2.5% 900мл", "шт", 48),
        Product.Create("P-002", "METRO-RICE-1KG", "Рис Metro Chef 1кг", "шт", 30),
        Product.Create("P-003", "RIOBA-COFFEE-1KG", "Кава Rioba 1кг", "шт", 20),
        Product.Create("P-004", "COCA-15", "Coca-Cola 1.5л", "шт", 60),
        Product.Create("P-005", "BARILLA-500", "Макарони Barilla 500г", "шт", 36),
        Product.Create("P-006", "WATER-15", "Вода мінеральна 1.5л", "шт", 72),
        Product.Create("P-007", "HEINZ-570", "Кетчуп Heinz 570г", "шт", 24),
        Product.Create("P-008", "ARO-TUNA-185", "Тунець ARO 185г", "шт", 40),
        Product.Create("P-009", "METRO-OIL-1L", "Олія Metro Chef 1л", "шт", 50),
        Product.Create("P-010", "RIOBA-TEA-100", "Чай Rioba 100 пакетиків", "уп", 15),
        Product.Create("P-011", "ARO-SUGAR-1KG", "Цукор ARO 1кг", "кг", 80),
        Product.Create("P-012", "METRO-FLOUR-1KG", "Борошно Metro Chef 1кг", "кг", 70),
        Product.Create("P-013", "ARO-SALT-1KG", "Сіль ARO 1кг", "кг", 90),
        Product.Create("P-014", "ARO-JUICE-1L", "Сік ARO 1л", "шт", 45),
        Product.Create("P-015", "METRO-PASTA-1KG", "Макарони Metro Chef 1кг", "шт", 35)
    };
}
