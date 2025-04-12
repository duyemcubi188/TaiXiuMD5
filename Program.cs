using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

class Program
{
    static async Task Main()
    {
        var botClient = new TelegramBotClient("8024673139:AAF0rlcv3_khqPYtmUowQu7e090tZ8cP1BM");

        var cancellationToken = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // nhận tất cả các loại update
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken.Token
        );

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Bot @{me.Username} đang chạy...");

        Console.ReadLine();
        cancellationToken.Cancel(); // Dừng bot
    }

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Message is not { Text: { } messageText }) return;

        var chatId = update.Message.Chat.Id;
        var input = messageText.Trim().ToLower();

        if (IsValidMd5(input))
        {
            int sumBytes;
            char lastChar;
            string result = PredictTaiXiuDetail(input, out sumBytes, out lastChar);

            string msg = $"🎲 *Kết quả dự đoán tài xỉu từ MD5*\n\n" +
                         $"🔐 Mã MD5: `{input}`\n" +
                         $"🧮 Tổng byte (3 byte cuối): {sumBytes}\n" +
                         $"🔢 Ký tự cuối của 4 ký tự cuối: `{lastChar}`\n\n" +
                         $"🎯 *Kết luận*: {result.ToUpper()}\n\n" +
                         $"👨‍💻 Nhà phát triển: Ngô Đức Duy\n" +
                         $"cre: duyemcubi188";

            await bot.SendTextMessageAsync(chatId, msg, parseMode: ParseMode.Markdown);
        }
        else
        {
            await bot.SendTextMessageAsync(chatId, "Vui lòng gửi chuỗi MD5 hợp lệ (32 ký tự hex).");
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Lỗi: {exception.Message}");
        return Task.CompletedTask;
    }

    static bool IsValidMd5(string input)
    {
        return input.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(input, @"^[0-9a-f]{32}$");
    }

    static string PredictTaiXiuDetail(string md5, out int result, out char lastChar)
    {
        if (md5.Length != 32)
            throw new ArgumentException("Chuỗi MD5 không hợp lệ. Phải có đúng 32 ký tự.");

        // Lấy 2 byte cuối (4 ký tự cuối cùng)
        string byte1Hex = md5.Substring(28, 2); // Byte thứ 1
        string byte2Hex = md5.Substring(30, 2); // Byte thứ 2

        // Chuyển từ hex sang số nguyên
        int byte1 = Convert.ToInt32(byte1Hex, 16);
        int byte2 = Convert.ToInt32(byte2Hex, 16);

        // Cộng 2 byte lại
        int total = byte1 + byte2;

        // Lấy 2 số cuối
        result = total % 100;

        // Áp dụng các điều kiện trừ theo ngưỡng
        if (result >= 24)
            result -= 24;
        else if (result >= 18)
            result -= 18;
        else if (result >= 12)
            result -= 12;
        else if (result >= 5)
            result -= 5;

        // Lấy ký tự cuối cùng của chuỗi MD5
        lastChar = md5[31];

        // Kiểm tra chẵn/lẻ
        return result % 2 == 0 ? "Xỉu" : "Tài";
    }
}
