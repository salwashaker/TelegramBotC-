using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;


namespace Weather
{
    class Program
    {
        private static TelegramBotClient? Bot;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("5066437923:AAGaJJgjntrQ2xI3fzjDeXx0v2B6ZbhvyTg");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "My weather Bot";

            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine($"listening for @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }


        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => OnMessage(botClient, update.Message!),
                UpdateType.EditedMessage => OnMessage(botClient, update.EditedMessage!),
                _ => UnknownUpdateHandler(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static Task UnknownUpdateHandler(ITelegramBotClient botC, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        private static async Task OnMessage(ITelegramBotClient botC, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;




            var action = message.Text switch
            {
                "/Use_keyboard" => SendReplyKeyboard(botC, message),
                "/Get_help" => getHelp(botC, message),
                "weather" or "Weather" => setAnswer(botC, message),
                _ => HowToUse(botC, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"message id: {sentMessage.MessageId}");


            static async Task<Message> SendReplyKeyboard(ITelegramBotClient botC, Message message)
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new(
                    new[]
                    {
                        new KeyboardButton[] { "Weather" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await botC.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Click on the keyboard",
                                                            replyMarkup: replyKeyboardMarkup);
            }





            static async Task<Message> setAnswer(ITelegramBotClient botC, Message message)
            {
                string[] msg = new string[5];
                //msg[0]: datatime
                //msg[1]: description
                //msg[2]:rand_temp - temperature
                //msg[3]:wind speed
                //msg[4]:the expressive image link

                //get date and time
                string now = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
                string datatime = "Time: " + now;
                msg[0] = datatime;


                //Generate Random temprature
                Random rd = new Random();
                int rand_temp = rd.Next(-5, 50);
                msg[2] = rand_temp.ToString();



                if (rand_temp <= 10)
                {
                    msg[1] = "stormy";
                    msg[3] = "47";
                    msg[4] = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f8/Stormy_Weather_%286862230425%29.jpg/320px-Stormy_Weather_%286862230425%29.jpg";
                }
                else if (rand_temp > 10 && rand_temp > 25)
                {
                    msg[1] = "cool";
                    msg[3] = "10";
                    msg[4] = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3b/Cool_weather_in_Jammu.jpg/320px-Cool_weather_in_Jammu.jpg";
                }
                else
                {
                    msg[1] = "hot";
                    msg[3] = "4";
                    msg[4] = "https://upload.wikimedia.org/wikipedia/commons/thumb/0/0c/High_hot_weather_-_Canicula_mare_-_panoramio.jpg/320px-High_hot_weather_-_Canicula_mare_-_panoramio.jpg";
                }



                return await botC.SendPhotoAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    photo: msg[4],
                    caption: "Hello!\n" +
                                msg[0]+"\n"+
                                "City: Baghdad\n"+
                                "Discription: "+ msg[1] + "\n" +
                                "Temperature: " + msg[2] + "C\n" +
                                "Feels like: " + msg[2] + "C\n" +
                                "Wind Speed: " + msg[3] +"Kph",
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl(
                            "Click for more info",
                            "https://weather.com/weather/today/"))
                        );

            }



            static async Task<Message> getHelp(ITelegramBotClient botC, Message message)
            {

                Message helpmsg = await botC.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "To ask about the weather, you can type 'weather' or use the onboard keyboard. \n" +
                    "Thanks. "
                        );

                return await HowToUse(botC, message);
            }



            static async Task<Message> HowToUse(ITelegramBotClient botC, Message message)
            {
                const string Tips = "/Use_keyboard - Work with keyboard\n" +
                                     "/Get_help   - Get help";

                return await botC.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: Tips,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }




        


    }
}
