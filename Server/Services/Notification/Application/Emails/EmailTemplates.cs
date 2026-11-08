using System.Net;

namespace Application.Emails;

public static class EmailTemplates
{
    private const string Brand = "Amazon";

    public static string EmailConfirmation(string code, int ttlMinutes) => Layout(
        preheader: "Ваш код підтвердження пошти",
        accent: "#6366F1", accentEnd: "#8B5CF6", tint: "#EEF2FF",
        emoji: "✉️",
        title: "Підтвердіть вашу пошту",
        intro: $"Вітаємо в {Brand}! Залишився один крок: введіть цей код у застосунку, щоб завершити реєстрацію.",
        code: code,
        ttlMinutes: ttlMinutes,
        warning: null,
        footer: "Ви отримали цей лист, бо хтось зареєстрував акаунт із цією адресою. Якщо це були не ви, просто проігноруйте лист.");

    public static string PasswordReset(string code, int ttlMinutes) => Layout(
        preheader: "Код для скидання пароля",
        accent: "#F97316", accentEnd: "#EF4444", tint: "#FFF7ED",
        emoji: "🔐",
        title: "Скидання пароля",
        intro: "Ми отримали запит на скидання пароля. Введіть цей код у застосунку, щоб створити новий пароль.",
        code: code,
        ttlMinutes: ttlMinutes,
        warning: "🛡️ Нікому не повідомляйте цей код. Співробітники " + Brand + " ніколи його не запитують.",
        footer: "Якщо ви не робили цей запит, змініть пароль і проігноруйте лист: ваш акаунт у безпеці, поки код не використано.");

    private static string Layout(
        string preheader, string accent, string accentEnd, string tint, string emoji,
        string title, string intro, string code, int ttlMinutes, string? warning, string footer)
    {
        var warningBlock = warning is null ? "" : $"""
            <tr><td style="padding:0 40px 24px;">
              <div style="background:#FEF2F2;border-left:4px solid #EF4444;border-radius:8px;padding:12px 16px;font-size:13px;line-height:20px;color:#991B1B;">
                {WebUtility.HtmlEncode(warning)}
              </div>
            </td></tr>
            """;

        return $"""
            <!DOCTYPE html>
            <html lang="uk">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>{WebUtility.HtmlEncode(title)}</title>
            </head>
            <body style="margin:0;padding:0;background-color:#F3F4F6;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;">{WebUtility.HtmlEncode(preheader)}</div>
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color:#F3F4F6;padding:32px 12px;">
                <tr><td align="center">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0"
                         style="max-width:520px;background:#FFFFFF;border-radius:20px;overflow:hidden;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">

                    <tr><td align="center" bgcolor="{accent}"
                            style="background-color:{accent};background-image:linear-gradient(135deg,{accent},{accentEnd});padding:36px 24px;">
                      <div style="font-size:48px;line-height:56px;">{emoji}</div>
                      <div style="margin-top:8px;font-size:13px;font-weight:700;letter-spacing:3px;color:#FFFFFF;opacity:0.9;">{Brand.ToUpperInvariant()}</div>
                    </td></tr>

                    <tr><td style="padding:32px 40px 8px;text-align:center;">
                      <h1 style="margin:0;font-size:26px;line-height:32px;font-weight:800;color:#111827;">{WebUtility.HtmlEncode(title)}</h1>
                      <p style="margin:12px 0 0;font-size:15px;line-height:24px;color:#4B5563;">{WebUtility.HtmlEncode(intro)}</p>
                    </td></tr>

                    <tr><td align="center" style="padding:24px 40px;">
                      <div style="background:{tint};border:2px dashed {accent};border-radius:16px;padding:20px 12px;">
                        <div style="font-size:12px;font-weight:700;letter-spacing:2px;color:{accent};">ВАШ КОД</div>
                        <div style="margin-top:6px;font-family:'Courier New',Consolas,monospace;font-size:40px;line-height:48px;font-weight:800;letter-spacing:10px;color:#111827;">{WebUtility.HtmlEncode(code)}</div>
                      </div>
                      <p style="margin:16px 0 0;font-size:14px;color:#6B7280;">⏱ Код діє <b style="color:#111827;">{ttlMinutes} хв</b></p>
                    </td></tr>

                    {warningBlock}

                    <tr><td style="padding:0 40px 32px;text-align:center;">
                      <hr style="border:none;border-top:1px solid #E5E7EB;margin:0 0 20px;">
                      <p style="margin:0;font-size:12px;line-height:18px;color:#9CA3AF;">{WebUtility.HtmlEncode(footer)}</p>
                    </td></tr>
                  </table>
                  <p style="margin:16px 0 0;font-family:'Segoe UI',Arial,sans-serif;font-size:12px;color:#9CA3AF;">© {Brand}. Це автоматичний лист, відповідати на нього не потрібно.</p>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}