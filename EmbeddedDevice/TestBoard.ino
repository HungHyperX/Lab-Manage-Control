#include <Wire.h>
#include <U8g2lib.h>
#include <Adafruit_NeoPixel.h>
#include <ArduinoJson.h>
#include <rdm6300.h>

// OLED cấu hình
U8G2_SSD1306_128X32_UNIVISION_F_SW_I2C u8g2(U8G2_R0, /* clock=*/ 22, /* data=*/ 21, /* reset=*/ U8X8_PIN_NONE);

// Nút nhấn
#define SW1 23
#define SW2 25
#define SW3 26
#define SW4 27

// Loa
#define SPEAKER 13

// LED WS2812 cấu hình
#define LED_PIN     4
#define NUM_LEDS    12
Adafruit_NeoPixel pixels(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

// LED Colors
uint32_t ledColors[] = {
  pixels.Color(255, 0, 0),     // Đỏ
  pixels.Color(0, 0, 255),     // Xanh dương
  pixels.Color(0, 255, 0),     // Xanh lá
  pixels.Color(255, 255, 0),   // Vàng
  pixels.Color(255, 165, 0),   // Cam
  pixels.Color(128, 0, 128)    // Tím
};
int colorCount = sizeof(ledColors) / sizeof(ledColors[0]);
int currentColorIndex = 0;


// RFID RDM6300
#define RDM6300_RX_PIN 16
Rdm6300 rdm6300;
String currentTagID = "";

String serialMessage = "";
StaticJsonDocument<512> jsonDoc;
String keys[10];
String values[10];
int keyCount = 0;
int currentIndex = 0;

unsigned long lastButtonPress = 0;
const unsigned long debounceDelay = 250;

unsigned long lastLedUpdate = 0;
const unsigned long ledUpdateInterval = 100; // Thời gian delay giữa các bước LED
int ledStep = 0;

void setup() {
  Serial.begin(115200);
  delay(1000);
  Serial.println("ESP32 + OLED U8g2 + WS2812 + RFID");

  u8g2.begin();
  u8g2.setFont(u8g2_font_ncenB08_tr);
  u8g2.clearBuffer();
  u8g2.drawStr(0, 10, "Khoi dong...");
  u8g2.sendBuffer();

  pinMode(SW1, INPUT_PULLUP);
  pinMode(SW2, INPUT_PULLUP);
  pinMode(SW3, INPUT_PULLUP);
  pinMode(SW4, INPUT_PULLUP);

  pinMode(SPEAKER, OUTPUT);

  pixels.begin();
  pixels.clear();
  pixels.show();
  updateLedColor(ledColors[currentColorIndex]);
  // Khởi tạo RFID
  rdm6300.begin(RDM6300_RX_PIN);
}

void loop() {
  // Đọc JSON từ Serial nếu có
  if (Serial.available() > 0) {
    serialMessage = Serial.readStringUntil('\n');
    serialMessage.trim();
    if (serialMessage.length() > 0) {
      parseAndStoreJson(serialMessage);
      if (keyCount > 0) displayKeyValue();
    }
  }

  // Nếu chưa có serialMessage, chạy LED vòng tròn
  if (serialMessage.length() == 0) {
    if (millis() - lastLedUpdate >= ledUpdateInterval) {
      runLedCircle();
      lastLedUpdate = millis();
    }
  }

  unsigned long now = millis();

  // Nút SW1
  if (digitalRead(SW1) == LOW && now - lastButtonPress > debounceDelay) {
    if (keyCount > 0) {
      currentIndex = (currentIndex - 1 + keyCount) % keyCount;
      displayKeyValue();
      beep();
    }
    lastButtonPress = now;
  }

  // Nút SW2
  if (digitalRead(SW2) == LOW && now - lastButtonPress > debounceDelay) {
    if (keyCount > 0) {
      currentIndex = (currentIndex + 1) % keyCount;
      displayKeyValue();
      beep();
    }
    lastButtonPress = now;
  }

  // Nút SW3: chuyển màu lùi
if (digitalRead(SW3) == LOW && now - lastButtonPress > debounceDelay) {
  currentColorIndex = (currentColorIndex - 1 + colorCount) % colorCount;
  updateLedColor(ledColors[currentColorIndex]);
  //showMessage("Mau LED giam");
  beep();
  lastButtonPress = now;
}

// Nút SW4: chuyển màu tới
if (digitalRead(SW4) == LOW && now - lastButtonPress > debounceDelay) {
  currentColorIndex = (currentColorIndex + 1) % colorCount;
  updateLedColor(ledColors[currentColorIndex]);
  //showMessage("Mau LED tang");
  beep();
  lastButtonPress = now;
}


  // Đọc RFID
  if (rdm6300.get_new_tag_id()) {
    currentTagID = String(rdm6300.get_tag_id(), HEX);

    StaticJsonDocument<128> doc;
    doc["rfid"] = currentTagID;

    String jsonStr;
    serializeJson(doc, jsonStr);

    Serial.println(jsonStr); // Gửi JSON qua Serial

    beep(); // Bíp khi có thẻ mới
    displayKeyValue(); // Cập nhật OLED
  }

  delay(50);
}

void beep() {
  digitalWrite(SPEAKER, HIGH);
  delay(100);
  digitalWrite(SPEAKER, LOW);
}

void showMessage(String msg) {
  u8g2.clearBuffer();
  u8g2.drawStr(0, 10, "ESP32 Test");
  u8g2.drawStr(0, 20, msg.c_str());
  u8g2.sendBuffer();
}

void parseAndStoreJson(String jsonStr) {
  DeserializationError error = deserializeJson(jsonDoc, jsonStr);
  if (error) {
    Serial.print("JSON parse error: ");
    Serial.println(error.c_str());
    showMessage("JSON error");
    return;
  }

  keyCount = 0;
  currentIndex = 0;

  for (JsonPair kv : jsonDoc.as<JsonObject>()) {
    if (keyCount < 10) {
      keys[keyCount] = kv.key().c_str();
      values[keyCount] = kv.value().as<String>();
      keyCount++;
    }
  }

  Serial.println("JSON parsed:");
  for (int i = 0; i < keyCount; i++) {
    Serial.print(keys[i]);
    Serial.print(": ");
    Serial.println(values[i]);
  }
}

void displayKeyValue() {
  u8g2.clearBuffer();
  u8g2.drawStr(0, 10, "Serial JSON:");
  String displayStr = (keyCount > 0) ? keys[currentIndex] + ": " + values[currentIndex] : "";
  u8g2.drawStr(0, 20, displayStr.c_str());

  String rfidLine = "RFID: ";
  if (currentTagID.length() > 0) {
    rfidLine += currentTagID;
  }
  u8g2.drawStr(0, 30, rfidLine.c_str());

  u8g2.sendBuffer();

  Serial.print("OLED: ");
  Serial.println(displayStr);
}

void runLedCircle() {
  pixels.clear();

  int ledPos = ledStep % NUM_LEDS;
  //pixels.setPixelColor(ledPos, pixels.Color(0, 0, 255)); // Màu xanh
  updateLedColor(ledColors[currentColorIndex]);
  // Tùy chỉnh thêm màu "đuôi" nếu muốn đẹp hơn
  int prevLed = (ledPos - 1 + NUM_LEDS) % NUM_LEDS;
  pixels.setPixelColor(prevLed, pixels.Color(0, 0, 50)); // Đuôi mờ

  pixels.show();

  ledStep++;
}

void updateLedColor(uint32_t color) {
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, color);
  }
  pixels.show();
}

