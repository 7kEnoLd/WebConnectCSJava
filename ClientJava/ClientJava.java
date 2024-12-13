import java.io.BufferedReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.lang.reflect.Array;
import java.lang.reflect.Field;
import java.net.HttpURLConnection;
import java.net.URI;
import java.net.URISyntaxException;
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
            // set up the connection
            @SuppressWarnings("deprecation")

            // the first time to get the data
            URL url1 = new URL("http://129.211.26.167:80/api/my/receive");
            HttpURLConnection conn1 = (HttpURLConnection) url1.openConnection();
            conn1.setRequestMethod("POST");
            conn1.setRequestProperty("Content-Type", "application/json; utf-8");
            conn1.setRequestProperty("Accept", "application/json");
            conn1.setDoOutput(true);
            String jsonInputString = "\"1\"";

            try (OutputStream os = conn1.getOutputStream()) {
                byte[] input1 = jsonInputString.getBytes(StandardCharsets.UTF_8);
                os.write(input1, 0, input1.length);
            }

            // read the response
            StringBuilder response1 = new StringBuilder();
            try (BufferedReader br1 = new BufferedReader(
                    new InputStreamReader(conn1.getInputStream(), StandardCharsets.UTF_8))) {
                String responseLine1;
                while ((responseLine1 = br1.readLine()) != null) {
                    response1.append(responseLine1.trim());
                }
                System.out.println("Response from .NET API: ");
                System.out.println(response1.toString());
            }

            // parse the response
            ReceiveData receiveData = parseJson(response1.toString());

            // the second time to send the data
            URL url2 = new URI("http://129.211.26.167:80/api/my/process").toURL();
            HttpURLConnection conn2 = (HttpURLConnection) url2.openConnection();
            conn2.setRequestMethod("POST");
            conn2.setRequestProperty("Content-Type", "application/json; utf-8");
            conn2.setRequestProperty("Accept", "application/json");
            conn2.setDoOutput(true);

            String jsonInputString2 = "\"" + toJson(receiveData.interval) + "\"";

            try (OutputStream os = conn2.getOutputStream()) {
                byte[] input2 = jsonInputString2.getBytes(StandardCharsets.UTF_8);
                os.write(input2, 0, input2.length);
            }

            // read the response
            try (BufferedReader br2 = new BufferedReader(
                    new InputStreamReader(conn2.getInputStream(), StandardCharsets.UTF_8))) {
                StringBuilder response2 = new StringBuilder();
                String responseLine2;
                while ((responseLine2 = br2.readLine()) != null) {
                    response2.append(responseLine2.trim());
                }
                System.out.println("Response from .NET API: ");
                System.out.println(response2.toString());

                // 替换转义字符为实际换行符
                String formattedJson = response2.toString().replace("\\r\\n", System.lineSeparator());

                // 获取当前北京时间并格式化为 yyyy-MM-dd_HH-mm-ss
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd_HH-mm-ss");
                String currentTime = sdf.format(new Date());

                // 定义输出文件名
                String filePath = "output_" + currentTime + ".txt";

                // 写入文件
                try (FileWriter writer = new FileWriter(filePath)) {
                    writer.write(formattedJson);
                    System.out.println("JSON 已成功写入文件: " + filePath);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        } catch (IOException | URISyntaxException e) {
        }
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
    public int[][] timeInterval;

    // 参考第一次访问返回的数据构造
    public ScheduleInput(int[][] timeInterval) {
        this.timeInterval = timeInterval;
    }
}

class ReceiveData {
    public String status;
    public String remark;
    public List<String> lineName;
    public int[][] interval;
}
