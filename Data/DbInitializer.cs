using hazinDNS_v2.Models;

namespace hazinDNS_v2.Data
{
    public static class DbInitializer
    {
        public static void ResetAdminBalance(ApplicationDbContext context)
        {
            var admin = context.Users.FirstOrDefault(u => u.Username == "admin");
            if (admin != null)
            {
                admin.Balance = 0;
                context.SaveChanges();
            }
        }

        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            
            if (!context.Users.Any())
            {
                var users = new User[]
                {
                    new User
                    {
                        Username = "admin",
                        Password = "password",
                        Email = "admin@hazindns.com",
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "user",
                        Password = "password",
                        Email = "user@hazindns.com",
                        Role = "User"
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            if (!context.Products.Any())
            {
                var products = new Product[]
                {
                    new Product 
                    { 
                        Name = "Холодильник Samsung RF50",
                        Price = 49999.99M,
                        Description = "Двухкамерный холодильник с системой NoFrost, объем 500л",
                        ImageUrl = "/images/samsung-fridge.jpg",
                        Category = "Холодильники",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Стиральная машина LG F2J3NS0W",
                        Price = 35999.99M,
                        Description = "Стиральная машина с функцией пара, загрузка 6.5 кг",
                        ImageUrl = "/images/lg-washer.jpg",
                        Category = "Стиральные машины",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Телевизор Sony XR-65A80L",
                        Price = 199999.99M,
                        Description = "OLED телевизор 65 дюймов, 4K, Smart TV",
                        ImageUrl = "/images/sony-tv.jpg",
                        Category = "Телевизоры",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Микроволновая печь Panasonic NN-GT261W",
                        Price = 8999.99M,
                        Description = "Микроволновая печь с грилем, 20л",
                        ImageUrl = "/images/panasonic-microwave.jpg",
                        Category = "Микроволновые печи",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Пылесос Dyson V15",
                        Price = 49999.99M,
                        Description = "Беспроводной пылесос с лазером для обнаружения пыли",
                        ImageUrl = "/images/dyson-vacuum.jpg",
                        Category = "Пылесосы",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Кофемашина DeLonghi ECAM 370.95.T",
                        Price = 89999.99M,
                        Description = "Автоматическая кофемашина с капучинатором",
                        ImageUrl = "/images/delonghi-coffee.jpg",
                        Category = "Кофемашины",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Посудомоечная машина Bosch SMS44DI01T",
                        Price = 42999.99M,
                        Description = "Отдельностоящая посудомоечная машина, 12 комплектов",
                        ImageUrl = "/images/bosch-dishwasher.jpg",
                        Category = "Посудомоечные машины",
                        InStock = true
                    },
                    new Product 
                    { 
                        Name = "Духовой шкаф Electrolux EOE7P31X",
                        Price = 39999.99M,
                        Description = "Электрический духовой шкаф с паром",
                        ImageUrl = "/images/electrolux-oven.jpg",
                        Category = "Духовые шкафы",
                        InStock = true
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
} 