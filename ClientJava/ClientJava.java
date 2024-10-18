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
            // 小算例
            // String jsonInputString = "{\"timeInterval\": [10, 10, 10, 20], \"timeRunning\": [[5, 12, 10000, 10000], [10000, 10000, 10, 4], [6, 10000, 10000, 9], [10000, 5, 13, 10000]], \"timeTransfer\": [0, 0, 0, 0], \"totalTimeInterval\": 30, \"timeLimit\": 10000,\"solverTimeLimit\": 20}"; 
            // 大算例
            String jsonInputString = "{\"timeInterval\": [10, 10, 15, 25, 20, 11, 15, 10, 14, 17, 24], \"timeRunning\": [[10000, 10000, 19, 24, 10000, 32, 10000, 36, 10000, 45, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000]," +
                              "[10000, 10000, 42, 37, 10000, 29, 10000, 25, 23, 10000, 1000, 10000, 10000, 10000, 10000, 10000, 10000, 10000]," +
                              "[10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 18, 42, 58, 10000, 10000, 10000, 10000, 51]," +
                              "[10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 90, 72, 10000, 14, 26, 22, 10000, 49, 10000]," +
                              "[10000, 17, 22, 10000, 34, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000]," +
                              "[37, 17, 26, 31, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000]," +
                              "[10000, 10000, 10000, 10000, 61, 53, 51, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000]," +
                              "[10000, 10000, 10000, 10000, 10000, 10000, 55, 10000, 48, 10000, 21, 10000, 10000, 10000, 10000, 10000, 10000, 10000]," +
                              "[10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 16, 20, 32, 10000, 10000]," +
                              "[10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 20, 10000, 10000]," +
                              "[10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 20, 28, 10000, 10000, 10000, 10000, 10000]]," +
                              "\"timeTransfer\": [4, 3, 2, 6, 0, 2, 7, 0, 6, 0, 1, 1, 0, 0, 1, 12, 11, 3], \"totalTimeInterval\": 240, \"timeLimit\": 10000,\"solverTimeLimit\": 20.0}";
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

