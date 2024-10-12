import java.io.*;
import java.net.*;
import java.nio.charset.StandardCharsets;

public class ClientJava {
    public static void main(String[] args) {
        try {
            // 设置URL，注意确保你的.NET API已经启动并运行在这个地址和端口
            @SuppressWarnings("deprecation")
            URL url = new URL("http://localhost:5162/api/my/process");
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();

            // 设置请求方式为POST
            conn.setRequestMethod("POST");
            conn.setRequestProperty("Content-Type", "application/json; utf-8");
            conn.setRequestProperty("Accept", "application/json");
            conn.setDoOutput(true);

            // 发送JSON数据，确保与.NET的JsonModel匹配
            String jsonInputString = "{\"timeInterval\": [10, 10, 10, 20], \"timeRunning\": [[5, 12, 10000, 10000], [10000, 10000, 10, 4], [6, 10000, 10000, 9], [10000, 5, 13, 10000]], \"timeTransfer\": [0, 0, 0, 0], \"totalTimeInterval\": 30, \"timeLimit\": 10000}";
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

