import java.io.BufferedReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.lang.reflect.Array;
import java.lang.reflect.Field;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Date;
import java.util.List;

public class ClientJava {
    public static void main(String[] args) {
        try {
            String url1 = "http://129.211.26.167:80/api/my/receive";
            String jsonInput1 = "\"1\"";
            String response1 = sendPostRequest(url1, jsonInput1);

            if (response1 != null) {
                System.out.println("Response from .NET API:");
                System.out.println(response1);

                ReceiveData receiveData = parseJson(response1);

                if (receiveData != null && receiveData.interval != null) {
                    String jsonInput2 = "\"" + toJson(receiveData.interval) + "\"";
                    String url2 = "http://129.211.26.167:80/api/my/process-request";
                    String response2 = sendPostRequest(url2, jsonInput2);
                    System.out.println("Server response: " + response2); // taskId

                    String taskId = extractTaskId(response2);

                    // 轮询 GET 请求，直到任务完成
                    String getUrl = "http://129.211.26.167:80/api/my/process-request?taskId=" + taskId;

                    while (true) {
                        String response = sendGetRequest(getUrl);

                        if (response != null && isCode200(response)) {
                            // 任务完成，处理结果
                            System.out.println("Task completed: " + response);
                            // 读取逻辑
                            // 替换转义字符并写入文件
                            String formattedJson = response.replace("\\r\\n", System.lineSeparator());
                            // 提取 busScheduleResult 和 trainScheduleResult
                            String busScheduleResult = extractJsonField(formattedJson, "busScheduleResult");
                            String trainScheduleResult = extractJsonField(formattedJson, "trainScheduleResult");

                            // 将 busScheduleResult 写入 bus_schedule.txt
                            writeToFile("bus_schedule.txt", busScheduleResult);

                            // 将 trainScheduleResult 写入 train_schedule.txt
                            writeToFile("train_schedule.txt", trainScheduleResult);

                            System.out.println("数据已写入到文件 bus_schedule.txt 和 train_schedule.txt");
                            break; // 退出循环
                        } else {
                            // 任务仍在处理中，继续轮询
                            System.out.println("Task still processing...");
                            try {
                                Thread.sleep(60000); // 等待 1 分钟后继续轮询
                            } catch (InterruptedException e) {
                                e.printStackTrace();
                                break;
                            }
                        }
                    }
                }
            }

        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * 发送 POST 请求
     */
    private static String sendPostRequest(String urlString, String jsonInput) throws IOException {
        @SuppressWarnings("deprecation")
        URL url = new URL(urlString);
        HttpURLConnection connection = (HttpURLConnection) url.openConnection();
        connection.setRequestMethod("POST");
        connection.setRequestProperty("Content-Type", "application/json; utf-8");
        connection.setRequestProperty("Accept", "application/json");
        connection.setDoOutput(true);

        try (OutputStream os = connection.getOutputStream()) {
            byte[] input = jsonInput.getBytes(StandardCharsets.UTF_8);
            os.write(input, 0, input.length);
        }

        try (BufferedReader br = new BufferedReader(
                new InputStreamReader(connection.getInputStream(), StandardCharsets.UTF_8))) {
            StringBuilder response = new StringBuilder();
            String responseLine;
            while ((responseLine = br.readLine()) != null) {
                response.append(responseLine.trim());
            }
            return response.toString();
        }
    }

    // 创建 HTTP 连接并发起 GET 请求
    public static String sendGetRequest(String urlString) throws IOException {
        @SuppressWarnings("deprecation")
        URL url = new URL(urlString);

        // 打开连接
        HttpURLConnection connection = (HttpURLConnection) url.openConnection();

        // 设置请求方法为 GET
        connection.setRequestMethod("GET");

        // 设置请求头，模拟浏览器
        connection.setRequestProperty("Accept", "application/json");

        // 获取响应代码
        int responseCode = connection.getResponseCode();

        // 如果响应是 200 OK, 读取响应内容
        if (responseCode == HttpURLConnection.HTTP_OK) {
            try (BufferedReader br = new BufferedReader(
                    new InputStreamReader(connection.getInputStream(), StandardCharsets.UTF_8))) {
                StringBuilder response = new StringBuilder();
                String responseLine;
                while ((responseLine = br.readLine()) != null) {
                    response.append(responseLine.trim());
                }
                return response.toString(); // 返回响应内容
            }
        } else {
            return "Error: " + responseCode; // 返回错误信息
        }
    }

    public static String extractJsonField(String json, String fieldName) {
        String fieldKey = "\"" + fieldName + "\":\"";
        int startIndex = json.indexOf(fieldKey) + fieldKey.length();
        int endIndex = json.indexOf("\"", startIndex);
        return json.substring(startIndex, endIndex);
    }

    /**
     * 写入 JSON 数据到文件
     */
    private static void writeToFile(String name, String content) {
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd_HH-mm-ss");
            String currentTime = sdf.format(new Date());
            String filePath = "output_" + currentTime + "_" + name;

            try (FileWriter writer = new FileWriter(filePath)) {
                writer.write(content);
                System.out.println("JSON 已成功写入文件: " + filePath);
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    // 提取 taskId 的值
    public static String extractTaskId(String json) {
        // 查找 "taskId" 字段的开始位置
        String key = "\"taskId\":\"";
        int startIndex = json.indexOf(key);

        if (startIndex == -1) {
            return null; // 如果没有找到 taskId 字段，返回 null
        }

        // 计算 "taskId" 字段值的开始位置
        startIndex += key.length();

        // 查找 taskId 值的结束位置
        int endIndex = json.indexOf("\"", startIndex);

        if (endIndex == -1) {
            return null; // 如果没有找到结束的引号，返回 null
        }

        // 返回 taskId 值
        return json.substring(startIndex, endIndex);
    }

    public static boolean isCode200(String json) {
        // 查找 "code": 的位置
        int codeIndex = json.indexOf("\"code\":");

        if (codeIndex == -1) {
            // 如果找不到 "code"，返回 false
            return false;
        }

        // 从 "code": 之后开始提取数字
        int startIndex = codeIndex + 7; // 跳过 "code": 的长度
        int endIndex = json.indexOf(',', startIndex);

        if (endIndex == -1) {
            // 如果没有逗号，尝试到字符串末尾
            endIndex = json.indexOf('}', startIndex);
        }

        // 提取 code 值
        String codeValue = json.substring(startIndex, endIndex).trim();

        // 检查 code 是否等于 200
        return "200".equals(codeValue);
    }

    public static String toJson(Object obj) {
        if (obj == null) {
            return "null";
        }

        StringBuilder jsonBuilder = new StringBuilder();

        if (obj.getClass().isArray()) {
            // 处理数组
            jsonBuilder.append("[");
            int length = Array.getLength(obj);
            for (int i = 0; i < length; i++) {
                Object element = Array.get(obj, i);
                if (i > 0) {
                    jsonBuilder.append(", ");
                }
                jsonBuilder.append(toJson(element)); // 递归处理数组中的元素
            }
            jsonBuilder.append("]");
        } else if (obj instanceof Collection) {
            // 处理集合
            jsonBuilder.append("[");
            boolean first = true;
            for (Object element : (Collection<?>) obj) {
                if (!first) {
                    jsonBuilder.append(", ");
                }
                jsonBuilder.append(toJson(element)); // 递归处理集合中的元素
                first = false;
            }
            jsonBuilder.append("]");
        } else if (obj instanceof String) {
            // 处理字符串
            jsonBuilder.append("\"").append(obj).append("\"");
        } else if (obj instanceof Number || obj instanceof Boolean) {
            // 处理数字和布尔值
            jsonBuilder.append(obj);
        } else {
            // 处理对象
            jsonBuilder.append("{");

            Field[] fields = obj.getClass().getDeclaredFields();
            boolean first = true;
            for (Field field : fields) {
                field.setAccessible(true);
                try {
                    String fieldName = field.getName();
                    Object fieldValue = field.get(obj);

                    if (!first) {
                        jsonBuilder.append(", ");
                    }
                    jsonBuilder.append("\"").append(fieldName).append("\": ");
                    jsonBuilder.append(toJson(fieldValue)); // 递归处理字段值
                    first = false;
                } catch (IllegalAccessException e) {
                }
            }

            jsonBuilder.append("}");
        }

        // 在整个 JSON 字符串外部添加双引号
        return jsonBuilder.toString();
    }

    static ReceiveData parseJson(String json) {
        ReceiveData responseData = new ReceiveData();

        // 去掉最前面和最后面的双引号
        if (json.startsWith("\"") && json.endsWith("\"")) {
            json = json.substring(1, json.length() - 1); // 去掉最前面和最后面的双引号
        }

        // 去掉多余的空白字符和换行
        json = json.replaceAll("\\\\r\\\\n", "").replaceAll("\\\\n", "").trim();

        // 判断是否是合法的 JSON 格式
        if (json.startsWith("{") && json.endsWith("}")) {
            json = json.substring(1, json.length() - 1); // 去掉两边的花括号

            // 查找 "Status" 字段
            int statusIndex = json.indexOf("Status\\");
            if (statusIndex != -1) {
                int start = json.indexOf("\"", statusIndex + 9) + 1; // 跳过 "Status": 字符串
                int end = json.indexOf("\\", start);
                String status = json.substring(start, end);
                responseData.status = status;
            }

            // 查找 "Remark" 字段
            int remarkIndex = json.indexOf("Remark\\");
            if (remarkIndex != -1) {
                int start = json.indexOf("\"", remarkIndex + 9) + 1; // 跳过 "Remark": 字符串
                int end = json.indexOf("\\", start);
                String remark = json.substring(start, end);
                responseData.remark = remark;
            }

            // 查找 "LineName" 数组
            int lineNameIndex = json.indexOf("LineName\\");
            if (lineNameIndex != -1) {
                int start = json.indexOf("[", lineNameIndex + 11) + 1; // 跳过 "LineName": [
                int end = json.indexOf("]", start);
                String lineNamesStr = json.substring(start, end);
                String[] lineNamesArray = lineNamesStr.split("\",");
                List<String> lineNames = new ArrayList<>();
                for (String lineName : lineNamesArray) {
                    lineNames.add(lineName.replace("\\", "").replace(" ", "").replace("\"", ""));
                }
                responseData.lineName = lineNames;
            }

            // 查找 "Interval" 二维数组
            int intervalIndex = json.indexOf("Interval\\");
            if (intervalIndex != -1) {
                int start = json.indexOf("[", intervalIndex + 11) + 1; // 跳过 "Interval": [
                int end = json.lastIndexOf("]"); // 获取最后一个 "]" 作为结束位置
                String intervalsStr = json.substring(start, end);
                String[] intervalGroups = intervalsStr.split("\\],"); // 分割每一组
                int[][] intervals = new int[intervalGroups.length][];
                for (int i = 0; i < intervalGroups.length; i++) {
                    String[] values = intervalGroups[i].replace("[", "").replace("]", "").split(",");
                    int[] intervalArray = new int[values.length];
                    for (int j = 0; j < values.length; j++) {
                        intervalArray[j] = Integer.parseInt(values[j].trim());
                    }
                    intervals[i] = intervalArray;
                }
                responseData.interval = intervals;
            }
        }

        return responseData;
    }
}

class ScheduleInput {

    // 字段1：timeInterval，发车间隔数组，一个整数数组，取值范围为(0,最大时间范围)，表示每个时段的每条线路的发车间隔，例如第一个数据[0][0]=10表示第一个时段第一条线路的发车间隔为10分钟
    public String SessionId; // 用于区分不同会话
    public String Input; // 输入数据（JSON 格式的 int[][]）

    // 参考第一次访问返回的数据构造
    public ScheduleInput(String input, String i) {
        this.Input = input;
        this.SessionId = i;
    }
}

class ReceiveData {
    public String status;
    public String remark;
    public List<String> lineName;
    public int[][] interval;
}
