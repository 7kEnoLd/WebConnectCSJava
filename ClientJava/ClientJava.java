import java.io.*;
import java.net.*;
import java.nio.charset.StandardCharsets;

public class ClientJava {
    public static void main(String[] args) {
        try {
            // 设置URL
            @SuppressWarnings("deprecation")
            URL url = new URL("http://localhost:5162/api/my/process");
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();

            // 设置请求方式为POST
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json; utf-8");
            conn.setRequestProperty("Accept", "application/json");
            conn.setDoOutput(true);

            // 发送JSON数据
            String jsonInputString = "{\"content\": \"Hello from Java!\"}";
            try (OutputStream os = conn.getOutputStream()) {
                byte[] input = jsonInputString.getBytes(StandardCharsets.UTF_8);
                os.write(input, 0, input.length);
            }

            // 读取响应
            try (BufferedReader br = new BufferedReader(
                    new InputStreamReader(conn.getInputStream(), StandardCharsets.UTF_8))) {
                StringBuilder response = new StringBuilder();
                String responseLine;
                while ((responseLine = br.readLine()) != null) {
                    response.append(responseLine.trim());
                }
                System.out.println("Response from .NET API: " + response.toString());
            }

        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
