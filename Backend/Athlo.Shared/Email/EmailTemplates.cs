namespace Athlo.Shared.Email;

/// <summary>Builds branded HTML email bodies for Athlo transactional mail.</summary>
public static class EmailTemplates
{
    public static string PasswordReset(string resetToken) => Wrap(
        title: "Reset your password",
        bodyHtml: $"""
            <p style="margin:0 0 16px;color:#1a1a1a;font-size:16px;line-height:1.5;">
              We received a request to reset your ATHLO password.
            </p>
            <p style="margin:0 0 8px;color:#555;font-size:14px;">Use this one-time code:</p>
            <p style="margin:0 0 24px;padding:16px;background:#f4f4f5;border-radius:8px;font-family:ui-monospace,monospace;font-size:18px;letter-spacing:0.04em;text-align:center;color:#111;">
              {System.Net.WebUtility.HtmlEncode(resetToken)}
            </p>
            <p style="margin:0;color:#777;font-size:13px;line-height:1.5;">
              If you did not request this, you can ignore this email. The code expires in one hour.
            </p>
            """);

    public static string PlainPasswordReset(string resetToken) =>
        $"Use this code to reset your ATHLO password:\n\n{resetToken}\n\nIf you did not request this, you can ignore this email. The code expires in one hour.";

    private static string Wrap(string title, string bodyHtml) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head><meta charset="utf-8" /><meta name="viewport" content="width=device-width, initial-scale=1" /><title>{System.Net.WebUtility.HtmlEncode(title)}</title></head>
        <body style="margin:0;padding:0;background:#0b0f14;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#0b0f14;padding:32px 16px;">
            <tr><td align="center">
              <table role="presentation" width="100%" style="max-width:520px;background:#ffffff;border-radius:12px;overflow:hidden;">
                <tr><td style="padding:24px 28px;background:#111827;">
                  <div style="color:#22c55e;font-weight:800;font-size:22px;letter-spacing:0.06em;">ATHLO</div>
                </td></tr>
                <tr><td style="padding:28px;">
                  <h1 style="margin:0 0 16px;font-size:20px;color:#111;">{System.Net.WebUtility.HtmlEncode(title)}</h1>
                  {bodyHtml}
                </td></tr>
                <tr><td style="padding:16px 28px 24px;border-top:1px solid #eee;">
                  <p style="margin:0;color:#999;font-size:12px;">© ATHLO — train with purpose</p>
                </td></tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
