#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <Adafruit_NeoPixel.h>

// OLED cấu hình
#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 32
#define OLED_ADDR 0x3C
TwoWire I2COLED = TwoWire(0);
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &I2COLED, -1);

// Nút nhấn
#define SW1 23
#define SW2 25
#define SW3 26
#define SW4 27

// Loa
#define SPEAKER 13

// LED WS2812 cấu hình
#define LED_PIN     4
#define NUM_LEDS    4
Adafruit_NeoPixel pixels(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

// Biến lưu nội dung Serial cuối cùng
String serialMessage = "";

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("ESP32 + OLED + WS2812");

  // OLED khởi động
  I2COLED.begin(21, 22);
  if (!display.begin(SSD1306_SWITCHCAPVCC, OLED_ADDR)) {
    Serial.println("Khong tim thay OLED!");
    while (true);
  }

  display.clearDisplay();
  display.setTextSize(1);
  display.setTextColor(SSD1306_WHITE);
  display.setCursor(0, 0);
  display.println("Khoi dong...");
  display.display();

  // Nút nhấn
  pinMode(SW1, INPUT_PULLUP);
  pinMode(SW2, INPUT_PULLUP);
  pinMode(SW3, INPUT_PULLUP);
  pinMode(SW4, INPUT_PULLUP);

  // Loa
  pinMode(SPEAKER, OUTPUT);

  // LED WS2812 khởi động
  pixels.begin(); 
  pixels.clear();  
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, pixels.Color(0, 0, 255)); // Xanh dương
  }
  pixels.show();
}

void loop() {
  // Xử lý nút nhấn
  if (digitalRead(SW1) == LOW) {
    showMessage("SW1 nhan"); beep();
  }
  if (digitalRead(SW2) == LOW) {
    showMessage("SW2 nhan"); beep();
  }
  if (digitalRead(SW3) == LOW) {
    showMessage("SW3 nhan"); beep();
  }
  if (digitalRead(SW4) == LOW) {
    showMessage("SW4 nhan"); beep();
  }

  // Đọc dữ liệu từ Serial nếu có
  if (Serial.available() > 0) {
    serialMessage = Serial.readStringUntil('\n'); // Đọc 1 dòng
    serialMessage.trim(); // Loại bỏ ký tự xuống dòng
    if (serialMessage.length() > 0) {
      showSerialMessage(serialMessage);
    }
  }

  delay(100);
}

void beep() {
  digitalWrite(SPEAKER, HIGH);
  delay(100);
  digitalWrite(SPEAKER, LOW);
}

void showMessage(String msg) {
  display.clearDisplay();
  display.setCursor(0, 0);
  display.println("ESP32 Test");
  display.println(msg);
  display.display();
}

void showSerialMessage(String msg) {
  display.clearDisplay();
  display.setCursor(0, 0);
  display.println("Serial in:");
  display.println(msg);
  display.display();

  Serial.print("Hien thi tren OLED: ");
  Serial.println(msg);
}
