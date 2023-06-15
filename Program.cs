using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using System.Xml;

interface IAsset
{
    decimal CurrentPrice { get; set; }
    string Name { get; }
    string Category { get; }
}

interface IDividends : IAsset
{
    void PayDividend();
}

[AttributeUsage(AttributeTargets.Class)]
public class DescriptionAttribute : Attribute
{
    public string Description { get; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}

[Description("Класс, представляющий пользователя.")]
class User
{
    public string Username { get; }
    public decimal Balance { get; set; }
    public DateTime purchaseDate { get; set; }
    public Portfolio Portfolio { get; set;  }
    public string Description { get; set; }



    public User(string username, decimal initialBalance)
    {
        Username = username;
        Balance = initialBalance;
        purchaseDate = DateTime.Now;
        Portfolio = new Portfolio(this);
    }


    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("Сумма депозита должна быть больше нуля.");
            return;
        }

        Balance += amount;
        Console.WriteLine($"Внесено {amount}. Новый баланс: {Balance }");
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("Сумма для вывода должна быть больше нуля.");
            return;
        }

        if (amount > Balance)
        {
            Console.WriteLine("Недостаточно средств.");
            return;
        }

        Balance -= amount;
        Console.WriteLine($"Снято {amount}. Новый баланс: {Balance}");
    }

    public void PurchaseAsset(IAsset asset, int quantity)
    {
        decimal cost = asset.CurrentPrice * quantity;
        if(cost > Balance)
        {
            Console.WriteLine("Недостаточно средств для покупки выбранного количества активов.");
            return;
        }

        Balance -= cost;
        Console.WriteLine($"Приобретено {quantity} активов. Новый баланс: {Balance}");

        if(asset is IDividends dividendAsset)
        {
            for (int i = 0; i < quantity; i++)
            {
                Enumerable.Range(0, quantity).ToList().ForEach(_ => dividendAsset.PayDividend());
            }
        }
        if(asset is IAsset tradableAsset)
        {
            tradableAsset.CurrentPrice *= 0.9m;
        }
        if(asset is Stock stock)
        {
            Portfolio.AddPosition(stock, quantity, asset, purchaseDate);
        }
    }

    public void DisplayBalance()
    {
        Console.WriteLine($"Баланс пользователя {Username}: {Balance}");
    }

    public void BuyBond(string bondName, decimal investmentAmount, decimal FaceValue)
    {
        Bond newBond = new Bond(bondName, investmentAmount);
        Portfolio.AddBond(newBond);
        Console.WriteLine($"Облигация {bondName} успешно добавлена в портфель.");
    }

    public static void PrintDescription()
    {
        var type = typeof(User);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий облигацию.")]
class Bond : IAsset
{
    public string Name { get; set; }
    public decimal CurrentPrice { get; set; }
    public string Category { get; set; }

    public Bond(string name, decimal currentPrice)
    {
        Name = name;
        CurrentPrice = currentPrice;
    }
    public static void PrintDescription()
    {
        var type = typeof(Bond);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий акцию.")]
class Stock : IDividends
{
    public string Name { get; }
    public decimal CurrentPrice { get; set; }
    public string Category { get; set; }


    public Stock(string name, decimal currentPrice, string category)
    {
        Name = name;
        CurrentPrice = currentPrice;
        Category = category;
    }

    public void PayDividend()
    {
        Console.WriteLine($"Выплачены дивиденды по акции {Name}");
    }

    public static void PrintDescription()
    {
        var type = typeof(Stock);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий позицию в портфеле.")]
class Position
{
    public Stock Stock { get; }

    public int Quantity { get; set; }
    public Bond bond { get; set; }
    public int Price { get; set; }

    public IAsset Asset { get; set; }


    public DateTime PurchaseDate { get; set; }

    public Position(Stock stock, int quantity,IAsset asset ,DateTime purchaseDate)
    {
        Stock = stock;
        Quantity = quantity;
        PurchaseDate = purchaseDate;
        Asset = asset;
    }

    public decimal GetPositionValue()
    {
        return Stock.CurrentPrice * Quantity;
    }

    public static void PrintDescription()
    {
        var type = typeof(Position);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий портфель пользователя.")]
class Portfolio
{
    public User Owner { get; }

    public List<Position> Positions { get; private set; }
    public int Quantity { get; set; }
    public List<Investment> Holdings { get; }

    public List<Bond> BondList { get; set; }
    public List<IAsset> Assets { get; set; }

    List<Bond> Bonds = new List<Bond>();

    private decimal GetRandomPrice()
    {
        Random random = new Random();
        return random.Next(50, 200);
    }


    public decimal GetTotalValue()
    {
        return Assets.Sum(asset => asset.CurrentPrice);
    }

    public Portfolio(User owner)
    {
        Owner = owner;
        Positions = new List<Position>();
        BondList = new List<Bond>();
        Assets = new List<IAsset>();
        Holdings = new List<Investment>();
    }

    public void AddPosition(Stock stock, int quantity, IAsset asset, DateTime purchaseDate)
    {
        Position existingPosition = Positions.FirstOrDefault(p => p.Asset.Name == stock.Name);

        if (existingPosition != null)
        {
            existingPosition.Quantity += quantity;
        }
        else
        {
            Positions.Add(new Position(stock, quantity, asset, purchaseDate));
        }

        Console.WriteLine($"Добавлено {quantity} акций {stock.Name} в портфель пользователя {Owner.Username}.");
    }

    public void AddInvestment(string asset, int quantity, double price)
    {
        var investment = new Investment
        {
            Asset = asset,
            Quantity = quantity,
            Price = price
        };

        Holdings.Add(investment);
    }

    public double CalculatePortfolioValue()
    {
        double totalValue = 0;
        foreach (var investment in Holdings)
        {
            totalValue += investment.CalculateValue();
        }

        return totalValue;
    }

    public List<Investment> GetInvestmentsByAsset(string asset)
    {
        var filteredInvestments = Holdings.Where(investment => investment.Asset == asset).ToList();
        return filteredInvestments;
    }

    public void ChangeStockPrices(decimal[] newPrices)
    {
        if (newPrices.Length != Positions.Count)
        {
            Console.WriteLine("Неверное количество новых цен.");
            return;
        }

        Positions.Zip(newPrices, (position, price) =>
        {
            decimal parsedPrice;
            if (!decimal.TryParse(price.ToString(), out parsedPrice))
            {
                Console.WriteLine("Неверный формат цены акции.");
                return null;
            }

            position.Asset.CurrentPrice = parsedPrice;
            return position;
        }).ToList();
    }

    public void AddBond(Bond bond)
    {
        Positions.Add(new Position(null, 1, bond, DateTime.Now));
    }

    public void DisplayPortfolioState()
    {
        Console.WriteLine($"Портфель пользователя {Owner.Username}:");
        Console.WriteLine("------------------------");

        Positions.ForEach(position =>
        {
            Console.WriteLine($"Акции: {position.Stock.Name}");
            Console.WriteLine($"Количество: {position.Quantity}");
            Console.WriteLine($"Текущая цена: {position.Stock.CurrentPrice}");
            Console.WriteLine($"Значение позиции: {position.GetPositionValue()}");
            Console.WriteLine($"------------------------");
        });
        Console.WriteLine($"Общая стоимость портфеля: {GetTotalValue()}");
    }


    public void UpdatePositionPrices()
    {
        decimal[] newPrices = new decimal[Positions.Count];
        for (int i = 0; i < Positions.Count; i++)
        {
            newPrices[i] = GetRandomPrice();
        }
        ChangeStockPrices(newPrices);
        
        Console.WriteLine("Цены акций в порфеле обновлены");
    }

    public static void PrintDescription()
    {
        var type = typeof(Portfolio);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий позицию актива.")]
class AssetPosition
{
    public IAsset Asset { get; }
    public int Quantity { get; set; }
    public DateTime PurchaseDate { get; }

    public AssetPosition(IAsset asset, int quantity, DateTime purchaseDate)
    {
        Asset = asset;
        Quantity = quantity;
        PurchaseDate = purchaseDate;
    }

    public static void PrintDescription()
    {
        var type = typeof(AssetPosition);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий инвестицию.")]
public class Investment
{
    public string Asset { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }

    public double CalculateValue()
    {
        return Quantity * Price;
    }

    public static void PrintDescription()
    {
        var type = typeof(Investment);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Абстрактный класс, представляющий новость.")]
abstract class News
{
    public abstract void ImpactMarket();

    public static void PrintDescription()
    {
        var type = typeof(News);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий нейтральные новости.")]
class NeutralNews : News
{
    public override void ImpactMarket()
    {
        Console.WriteLine("Нейтральные новости. Рынок остается стабильным.");
    }

    public static void PrintDescription()
    {
        var type = typeof(NeutralNews);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий плохие новости.")]
class BadNews : News
{
    public override void ImpactMarket()
    {
        Console.WriteLine("Плохие новости. Цены акций снижаются на 10%");
    }

    public static void PrintDescription()
    {
        var type = typeof(BadNews);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий ужасные новости.")]
class TerribleNews : News
{
    public override void ImpactMarket()
    {
        Console.WriteLine("Ужасные новости. Одна из акций уничтожена,заменена на другую.");
    }

    public static void PrintDescription()
    {
        var type = typeof(TerribleNews);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий хорошие новости.")]
class GoodNews : News
{
    public override void ImpactMarket()
    {
        Console.WriteLine("Хорошие новости. Цены акций увеличиваются на 10%");
    }

    public static void PrintDescription()
    {
        var type = typeof(GoodNews);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}

[Description("Класс, представляющий рынок.")]
public class Market
{
    public string Name { get; set; }
    public decimal Value { get; set; }
    public bool IsMarketOpen { get; set; }

    private static readonly Lazy<Market> instance = new Lazy<Market> (() => new Market());

    private Market()
    {
    }

    public static Market Instance => instance.Value;
    public void PublishNews(string news)
    {
        Console.WriteLine($"Публикация новостей на рынке: {news}");
    }
    public static void PrintDescription()
    {
        var type = typeof(Market);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
    public Market(bool isMarketOpen)
    {
        IsMarketOpen = isMarketOpen;
    }
}

[Description("Класс, представляющий логирования")]
public class Logger
{
    private readonly string logFilePath;
    public Logger(string logFilePath)
    {
        this.logFilePath = logFilePath;
    }

    public void Log(string message)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        Console.WriteLine(logMessage);
    }

    public static void PrintDescription()
    {
        var type = typeof(Logger);
        var attribute = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
        if (attribute != null)
        {
            Console.WriteLine($"Description: {attribute.Description}");
        }
        else
        {
            Console.WriteLine("Description attribute not found.");
        }
    }
}


class Program
{
    private static Market market = Market.Instance;
    private static Logger logger;
    static Semaphore newsSemaphore = new Semaphore(1, 1);
    static Mutex dividendMutex = new Mutex(); 

    static Dictionary<string, List<(string Name, double Price)>> assets = new Dictionary<string, List<(string Name, double Price)>>()
    {
        { "1. Недвижимость", new List<(string, double)>() { ("1. Квартира", 100000), ("Дом", 200000), ("Офисное здание", 500000) } },
        { "2. Транспорт", new List<(string, double)>() { ("1. Автомобиль", 20000), ("Велосипед", 500), ("Мотоцикл", 15000) } },
        { "3. Техника", new List<(string, double)>() { ("1. Ноутбук", 1000), ("Смартфон", 800), ("Телевизор", 1500) } },
        { "4. Инструменты", new List<(string, double)>() { ("1. Молоток", 500), ("2. Отвертка", 300), ("3. Дрель", 3500) } },
        { "5. Финансовые инструменты", new List<(string, double)>() { ("1. Акции", 100000), ("2. Облигации", 10000), ("3. Фьючерсы", 1000) } },
        { "6. Ювелирные изделия", new List<(string, double)>() { ("1. Кольцо", 1000), ("2. Серьги", 700), ("3. Браслет", 1500) } },
        { "7. Искусство", new List<(string, double)>() { ("1. Картина", 5000), ("2. Скульптура", 10000), ("3. Фотография", 2000) } },
        { "8. Спортивное снаряжение", new List<(string, double)>() { ("1. Велосипед", 4250), ("2. Теннисная ракетка", 2500), ("3. Беговые кроссовки", 1200) } },
        { "9. Предприятия", new List<(string, double)>() { ("1. Ресторан", 300000000), ("2. Магазин", 20000000), ("3. Производственное предприятие", 15000000000) } },
        { "10. Криптовалюты", new List<(string, double)>() { ("1. Биткоин", 19850), ("2. Эфириум", 13053), ("3. Рипл", 9230) } }
    };

    static Dictionary<string, string> stockCategories = new Dictionary<string, string>()
    {
        { "Apple ", "Технологии" },
        { "Microsoft Corporation", "Технологии" },
        { "Johnson & Johnson", "Здравоохранение" },
        { "Coca-Cola Company", "Пищевая промышленность" },
        { "Walmart ", "Розничная торговля" },
        { "Nestle", "Пищевая промышленность" },
        { "Danone ", "Пищевая промышленность" },
        { "Mondelez International", "Пищевая промышленность" },
        { "Kraft Heinz Company", "Пищевая промышленность" },
        { "Procter & Gamble Company", "Бытовая химия" },
        { "The Clorox Company", "Бытовая химия" },
        { "Colgate-Palmolive Company", "Бытовая химия" },
        { "Unilever ", "Бытовая химия" },
        { "Henkel", "Бытовая химия" },
        { "Target Corporation", "Розничная торговля" },
        { "Costco Wholesale Corporation", "Розничная торговля" },
        { "The Home Depot ", "Розничная торговля" },
        { "Tesla", "Технологии" },
        { "Visa ", "Финансы" },
        { "JPMorgan Chase ", "Финансы" },
        { "Bank of America Corporation", "Финансы" },
        { "Citigroup ", "Финансы" },
        { "Wells Fargo & Company", "Финансы" },
        { "Goldman Sachs Group ", "Финансы" },
        { "Alphabet", "Технологии" },
        { "Johnson Controls International plc", "Промышленное оборудование" },
        { "Facebook", "tТехнологии" },
        { "Verizon Communications ", "Телекоммуникации" },
        { "Merck & Co ", "Здравоохранение" },
        { "Abbott Laboratories", "Здравоохранение" },
    };

    static void PrintAllCategories()
    {
        Console.WriteLine("Все категории активов:");
        foreach (string category in assets.Keys)
        {
            Console.WriteLine(category);
        }
    }

    static void PrintAssetsInCategory(string category)
    {
        Console.WriteLine($"Активы в категории {category}:");
        double totalValue = 0;

        foreach (var asset in assets[category])
        {
            Console.WriteLine($"{asset.Name} (${asset.Price})");
            totalValue += asset.Item2;
        }
        Console.WriteLine($"Общая стоимость категории {category}: ${totalValue}");

    }

    static void SearchRelatedAssets(string assetName)
    {
        Console.WriteLine($"Связанные активы с '{assetName}':");

        foreach (var category in assets)
        {
            foreach (var asset in category.Value)
            {
                if (asset.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"{asset.Name} (${asset.Price}) - Категория: {category.Key}");
                }
            }
        }
    }

    static void SearchRelatedStocks()
    {
        Console.Write("Введите актив для поиска связанных активов: ");
        string stockName = Console.ReadLine();

        List<string> relatedStocks = GetRelatedStocks(stockName);

        Console.WriteLine($"Связанные активы для актива \"{stockName}\":"); 
        foreach (var stock in relatedStocks)
        {
            Console.WriteLine(stock);
        }
    }

    static List<string> GetRelatedStocks(string stockName)
    {
        List<string> relatedStocks = new List<string>();

        string category = stockCategories.ContainsKey(stockName) ? stockCategories[stockName] : null;

        if (category != null)
        {
            foreach (var stock in stockCategories)
            {
                if (stock.Value == category)
                {
                    relatedStocks.Add(stock.Key);
                }
            }
        }

        return relatedStocks;
    }

    static bool isProgramRunning = true;

    static bool isOpenMarket = false;

    static async Task Main(string[] args)
    {
        Console.Write("Введите имя рынка: ");
        market.Name = Console.ReadLine();

        Console.Write("Введите текущую стоимость рынка: ");
        market.Value = Convert.ToDecimal(Console.ReadLine());
        bool IsMarketOpen = false;
        while (!IsMarketOpen)
        {
            Console.Write("Рынок открыт? (true/false): ");
            string marketOpenInput = Console.ReadLine();
            if (bool.TryParse(marketOpenInput, out IsMarketOpen))
            {
                market.IsMarketOpen = IsMarketOpen;
            }
            else
            {
                Console.WriteLine("Неверный ввод. Пожалуйста, введите 'true' или 'false'.");
            }
        }
        Console.WriteLine("Программа продолжает работу...");

        User.PrintDescription();
        Bond.PrintDescription();
        Stock.PrintDescription();
        Position.PrintDescription();
        Portfolio.PrintDescription();
        AssetPosition.PrintDescription();
        Investment.PrintDescription();
        News.PrintDescription();
        NeutralNews.PrintDescription();
        BadNews.PrintDescription();
        TerribleNews.PrintDescription();
        GoodNews.PrintDescription();
        Logger.PrintDescription();

        Task newsTask = GenerateNewsAsync();
        Task dividendTask = PayDividendsAsync();

        Console.WriteLine("Программа выполняет другие действия...");

        DateTime purchaseDate = new DateTime(2023, 5, 30);
        List<News> newsList = new List<News>
        {
            new BadNews(),  
            new TerribleNews(),
            new GoodNews(),
        };

        Random random = new Random();

        Console.WriteLine("Введите имя пользователя: ");
        string username = Console.ReadLine();

        Console.WriteLine("Введите начальный баланс: ");
        decimal initialBalance;
        if (!decimal.TryParse(Console.ReadLine(), out initialBalance))
        {
            Console.WriteLine("Неверный формат баланса.");
            return;
        }

        User newUser = new User(username, initialBalance);
        Console.WriteLine($"Пользователь {newUser.Username} создан с балансом {newUser.Balance},  был создан: {newUser.purchaseDate}");

        TimerCallback callback = new TimerCallback(UpdateStockPrices);
        Timer timer = new Timer(callback, newUser.Portfolio, 0, 50000);
        Console.WriteLine("Изменение цен акций...");
        
        Task Market = Task.Run(() => MarketTask());

        bool continueProgram = true;

        foreach (var _ in Enumerable.Range(0, int.MaxValue).TakeWhile(_ => continueProgram))
        {
            Console.WriteLine("Выберите действие из меню!!!:");
            Console.WriteLine("1. Пополнить баланс");
            Console.WriteLine("2. Вывести средства");
            Console.WriteLine("3. Показать баланс");
            Console.WriteLine("4. Открыть рынок");
            Console.WriteLine("5. Закрыть рынок");
            Console.WriteLine("6. Управление портфелем");
            Console.WriteLine("7. Вызвать новость");
            Console.WriteLine("8. Выйти из программы");

            int subChoice;
            if (!int.TryParse(Console.ReadLine(), out subChoice))
            {
                Console.WriteLine("Неверный выбор.");
                continue;
            }

            switch (subChoice)
            {
                case 1:
                    Console.WriteLine("Введите сумму для пополнения:");
                    decimal depositAmount;
                    if (!decimal.TryParse(Console.ReadLine(), out depositAmount))
                    {
                        Console.WriteLine("Неверный формат суммы.");
                        continue;
                    }
                    newUser.Deposit(depositAmount);
                    break;

                case 2:
                    Console.WriteLine("Введите сумму для вывода:");
                    decimal withdrawAmount;
                    if (!decimal.TryParse(Console.ReadLine(), out withdrawAmount))
                    {
                        Console.WriteLine("Неверный формат суммы.");
                        continue;
                    }
                    newUser.Withdraw(withdrawAmount);
                    break;

                case 3:
                    User.PrintDescription();
                    newUser.DisplayBalance();
                    break;
                case 4:
                    await OpenMarket();
                    break;
                case 5:
                    await CloseMarket();
                    break;
                case 6:
                    Portfolio.PrintDescription();
                    if (isOpenMarket)
                    {
                        bool continueBuyingStocks = true;

                        while (continueBuyingStocks)
                        {
                            Console.WriteLine("Выберите действие из меню!:");
                            Console.WriteLine("1. Купить акции");
                            Console.WriteLine("2. Купить облигаций");
                            Console.WriteLine("3. Показать портфель");
                            Console.WriteLine("4. Обновить цены акций в портфеле");
                            Console.WriteLine("5. Вывести все категории активов");
                            Console.WriteLine("6. Выйти");

                            int сhoice = 0;
                            if (!int.TryParse(Console.ReadLine(), out сhoice))
                            {
                                Console.WriteLine("Неверный выбор.");
                                continue;
                            }
                            switch (сhoice)
                            {
                                case 1:
                                    Stock.PrintDescription();
                                    BuyStock(newUser);
                                    break;

                                case 2:
                                    Bond.PrintDescription();
                                    BuyBond(newUser);
                                    break;
                                case 3:
                                    Position.PrintDescription();
                                    newUser.Portfolio.DisplayPortfolioState();
                                    break;
                                case 4:
                                    newUser.Portfolio.UpdatePositionPrices();
                                    break;
                                case 5:
                                    bool exit = false;

                                    while (!exit)
                                    {
                                        Console.WriteLine("Меню:");
                                        Console.WriteLine("1. Вывести все категории активов");
                                        Console.WriteLine("2. Вывести активы в выбранной категории");
                                        Console.WriteLine("3. Поиск активов по связанному активу");
                                        Console.WriteLine("4. Поиск акции по связанному активу");
                                        Console.WriteLine("0. Выход");

                                        Console.Write("Выберите пункт меню: ");
                                        string input = Console.ReadLine();

                                        switch (input)
                                        {
                                            case "1":
                                                PrintAllCategories();
                                                break;
                                            case "2":
                                                Console.WriteLine("Выберите категорию:");
                                                PrintAllCategories();
                                                Console.Write("Введите номер категории: ");
                                                string categoryInput = Console.ReadLine();
                                                if (int.TryParse(categoryInput, out int categoryIndex))
                                                {
                                                    if (categoryIndex >= 1 && categoryIndex <= assets.Count)
                                                    {
                                                        string category = assets.Keys.ElementAt(categoryIndex - 1);
                                                        PrintAssetsInCategory(category);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine("Неверный номер категории.");
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Неверный ввод. Попробуйте снова.");
                                                }
                                                break;
                                            case "3":
                                                Console.Write("Введите актив для поиска: ");
                                                string searchAsset = Console.ReadLine();
                                                SearchRelatedAssets(searchAsset);
                                                break;
                                            case "4":
                                                SearchRelatedStocks();
                                                break;
                                            case "0":
                                                exit = true;
                                                break;
                                            default:
                                                Console.WriteLine("Неверный ввод. Попробуйте снова.");
                                                break;
                                        }

                                        Console.WriteLine();
                                    }

                                    break;
                                case 6:
                                    continueBuyingStocks = false;
                                    break;
                                default:
                                    Console.WriteLine("Неверный выбор.");
                                    break;
                            }
                            break;
                        }
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Рынок закрыт. Управление портфелем недоступно.");
                    }
                    break;
                case 7:
                    News.PrintDescription();
                    NeutralNews.PrintDescription();
                    BadNews.PrintDescription();
                    TerribleNews.PrintDescription();
                    GoodNews.PrintDescription();
                    TriggerRandomNews(newUser.Portfolio, newsList, random);
                    break;
                case 8:
                    continueProgram = false;
                    break;

                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }
        }

        int randomIndex = random.Next(newsList.Count);
        News randomNews = newsList[randomIndex];
        randomNews.ImpactMarket();

        newUser.DisplayBalance();
        newUser.Portfolio.DisplayPortfolioState();

        List<decimal> newPrices = new List<decimal> { 170, 500, 1800, 350, 600, 200, 1500, 400, 160, 220, 5000, 10000, 50000, 100000, 500000 };
        newUser.Portfolio.ChangeStockPrices(newPrices.ToArray());

        newUser.DisplayBalance();
        newUser.Portfolio.DisplayPortfolioState();

        Portfolio portfolio = new Portfolio(newUser);
        portfolio.DisplayPortfolioState();

        TriggerRandomNews(portfolio, newsList, random);


        isProgramRunning = false;

        await newsTask;
        await dividendTask;

        logger = new Logger("logs.txt");

        await PerformMarketAction(market);
        await SaveMarketState();
        await LoadMarketState();

        Console.WriteLine("Программа завершена.");
        Console.WriteLine("Нажмите любую клавишу для завершения программы...");
        Console.ReadKey();
        bool isMarketSaved = File.Exists("market_state.json");
        Console.WriteLine("Состояние рынка сохранено: " + isMarketSaved);
    }

    private static async Task PerformMarketAction(Market market)
    {
        if (market.IsMarketOpen)
        {
            Console.WriteLine("Выполняется действие на открытом рынке...");
            logger.Log("Выполнено действие на открытом рынке.");
        }
        else
        {
            Console.WriteLine("Выполняется действие на закрытом рынке...");
            logger.Log("Выполнено действие на закрытом рынке.");
        }
    }

    private static async Task SaveMarketState()
    {
        string json = JsonConvert.SerializeObject(market, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("market_state.json", json);

        logger.Log("Состояние рынка сохранено.");
    }

    private static async Task LoadMarketState()
    {
        try
        {
            if (File.Exists("market_state.json"))
            {
                string json = File.ReadAllText("market_state.json");
                market = JsonConvert.DeserializeObject<Market>(json);

                Console.WriteLine("Состояние рынка успешно загружено.");
            }
            else
            {
                Console.WriteLine("Файл состояния рынка не найден.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при загрузке состояния рынка: " + ex.Message);
        }
    }


    static async Task OpenMarket()
    {
        if (!isOpenMarket)
        {
            Console.WriteLine("Открытие рынка...");

            isOpenMarket = true;

            await Task.Delay(1000);

            Console.WriteLine("Рынок открыт");
        }
        else
        {
            Console.WriteLine("Рынок уже открыт");
        }
    }

    static async Task CloseMarket()
    {
        if (isOpenMarket)
        {
            Console.WriteLine("Закрытие рынка...");

            isOpenMarket = false;

            await Task.Delay(1000);

            Console.WriteLine("Рынок закрыт");
        }
        else
        {
            Console.WriteLine("Рынок уже закрыт");
        }
    }

    static async Task MarketTask()
    {
        Console.WriteLine("Задача 'Рынок' запущена");

        await Task.Run(async () =>
        {
            while (true)
            {
                if (isOpenMarket)
                {
                    Console.WriteLine("Рынок работает...");

                    await Task.Delay(50000);
                }
                else
                {
                    Console.WriteLine("Рынок закрыт...");

                    await Task.Delay(50000);
                }
            }
        });
    }

    static async Task GenerateNewsAsync()
    {

        await Task.Run(async () =>
        {
            while (isProgramRunning)
            {
                string news = await GenerateRandomNewsAsync();
                Console.WriteLine(news);

                await Task.Delay(12000);
            }
        });
    }

    static async Task<string> GenerateRandomNewsAsync()
    {
        
        string[] newsList = {
            "Публикация новостей на рынке: Нейтральные новости. Рынок остается стабильным.",
            "Публикация новостей на рынке: Плохие новости. Цены акций снижаются на 10%.",
            "Публикация новостей на рынке: Ужасные новости. Одна из акций уничтожена, заменена на другую.",
            "Публикация новостей на рынке: Хорошие новости. Цены акций увеличиваются на 10%."
        };

        Random random = new Random();
        int index = random.Next(newsList.Length);

        
        await Task.Delay(5000);

        return newsList[index];
    }
    static async Task PayDividendsAsync()
    {

        await Task.Run(async () =>
        {
            while (isProgramRunning)
            {
                Console.WriteLine("Выплачены дивиденды");

                await Task.Delay(12000);
            }
        });
    }

    static void TriggerRandomNews(Portfolio portfolio, List<News> newsList, Random random)
    {
        int randomIndex = random.Next(newsList.Count);
        News randomNews = newsList.OrderBy(_ => random.Next()).FirstOrDefault();
        randomNews?.ImpactMarket();

        portfolio.UpdatePositionPrices();

        Console.WriteLine();
    }

    static void UpdateStockPrices(object state)
    {
        Portfolio portfolio = (Portfolio)state;
        portfolio.UpdatePositionPrices();
    }

    private static void BuyStock(User user)
    {
        Console.WriteLine("Выберите акцию для покупки:");
        Console.WriteLine("1. Apple Inc. (AAPL)" + "\tТехнологии");
        Console.WriteLine("2. Tesla Inc. (TSLA)" + "\tТехнологии");
        Console.WriteLine("3. Amazon.com Inc. (AMZN)" + "\tТехнологии");
        Console.WriteLine("4. Microsoft Corporation (MSFT)" + "\tТехнологии");
        Console.WriteLine("5. Alphabet Inc. (GOOGL)" + "\tТехнологии");
        Console.WriteLine("6. Facebook Inc. (FB)" + "\tТехнологии");
        Console.WriteLine("7. Netflix Inc. (NFLX)" + "\tТехнологии");
        Console.WriteLine("8. NVIDIA Corporation (NVDA)" + "\tТехнологии");
        Console.WriteLine("9. Johnson & Johnson (JNJ)" + "\tЗдравоохранение");
        Console.WriteLine("10. Visa Inc. (V)" + "\tФинансы");

        int stockChoice;
        if (!int.TryParse(Console.ReadLine(), out stockChoice))
        {
            Console.WriteLine("Неверный выбор.");
            return;
        }

        Stock selectedStock = null;

        switch (stockChoice)
        {
            case 1:
                selectedStock = new Stock("Apple Inc.", 150, "Технологии");
                break;
            case 2:
                selectedStock = new Stock("Tesla Inc.", 700, "Технологии");
                break;
            case 3:
                selectedStock = new Stock("Amazon.com Inc.", 3000, "Технологии");
                break;
            case 4:
                selectedStock = new Stock("Microsoft Corporation", 250, "Технологии");
                break;
            case 5:
                selectedStock = new Stock("Alphabet Inc.", 1800, "Технологии");
                break;
            case 6:
                selectedStock = new Stock("Facebook Inc.", 350, "Технологии");
                break;
            case 7:
                selectedStock = new Stock("Netflix Inc.", 500, "Технологии");
                break;
            case 8:
                selectedStock = new Stock("NVIDIA Corporation", 600, "Технологии");
                break;
            case 9:
                selectedStock = new Stock("Johnson & Johnson", 170, "Здравоохранение");
                break;
            case 10:
                selectedStock = new Stock("Visa Inc.", 250, "Финансы");
                break;
            default:
                Console.WriteLine("Неверный выбор.");
                return;
        }

        Console.WriteLine("Введите количество акций для покупки:");

        int quantity;
        if (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверный формат количества.");
            return;
        }

        decimal totalPrice = selectedStock.CurrentPrice * quantity;

        if (totalPrice > user.Balance)
        {
            Console.WriteLine("Недостаточно средств для покупки выбранного количества акций.");
            return;
        }

        user.PurchaseAsset(selectedStock, quantity);
        if (selectedStock is IDividends dividendStock)
        {
            for (int i = 0; i < quantity; i++)
            {
                dividendStock.PayDividend();
            }
        }

        user.Balance -= totalPrice;
        Console.WriteLine($"Поздравляем, вы купили {quantity} акций {selectedStock.Name} за {totalPrice}$.");
        Console.WriteLine($"Акции успешно приобретены. Баланс пользователя: {user.Balance}, Дата покупки: {user.purchaseDate}");
    }

    static void BuyBond(User user)
    {
        Console.WriteLine("Выберите облигацию из списка:");
        Console.WriteLine("1. Газпром.");
        Console.WriteLine("2. Роснефть");
        Console.WriteLine("3. Сбербанк");
        Console.WriteLine("4. Лукойл");
        Console.WriteLine("5. РЖД");
        Console.WriteLine("6. Газпром нефть");
        Console.WriteLine("7. РусГидро");
        Console.WriteLine("8. МТС");
        Console.WriteLine("9. Ростелеком");
        Console.WriteLine("10. Транснефть");

        int bondChoice;
        if (!int.TryParse(Console.ReadLine(), out bondChoice))
        {
            Console.WriteLine("Неверный выбор.");
            return;
        }

        Bond selectedBond = null;

        switch (bondChoice)
        {
            case 1:
                selectedBond = new Bond("Газпром", 700);
                break;
            case 2:
                selectedBond = new Bond("Роснефть", 600);
                break;
            case 3:
                selectedBond = new Bond("Сбербанк", 500);
                break;
            case 4:
                selectedBond = new Bond("Лукойл", 400);
                break;
            case 5:
                selectedBond = new Bond("РЖД", 410);
                break;
            case 6:
                selectedBond = new Bond("Газпром нефть", 1000);
                break;
            case 7:
                selectedBond = new Bond("РусГидро", 430);
                break;
            case 8:
                selectedBond = new Bond("МТС", 450);
                break;
            case 9:
                selectedBond = new Bond("Ростелеком", 500);
                break;
            case 10:
                selectedBond = new Bond("Транснефть", 650);
                break;
            default:
                Console.WriteLine("Неверный выбор.");
                return;
        }

        Console.WriteLine("Введите сумму для покупки облигаций:");

        decimal amount;
        if (!decimal.TryParse(Console.ReadLine(), out amount) || amount <= 0)
        {
            Console.WriteLine("Неверный формат суммы.");
            return;
        }

        decimal totalPrice = selectedBond.CurrentPrice * amount;

        if (totalPrice > user.Balance)
        {
            Console.WriteLine("Недостаточно средств для покупки выбранной суммы облигаций.");
            return;
        }

        user.PurchaseAsset(selectedBond, (int)amount);
        Console.WriteLine($"Облигации успешно приобретены: {selectedBond.Name}. Баланс пользователя: {user.Balance}, Дата покупки: {user.purchaseDate}");
    }
}
