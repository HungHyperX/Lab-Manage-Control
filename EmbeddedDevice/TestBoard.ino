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
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, pixels.Color(0, 0, 255));
  }
  pixels.show();

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

  // Nút SW3
  if (digitalRead(SW3) == LOW && now - lastButtonPress > debounceDelay) {
    showMessage("SW3 nhan");
    beep();
    lastButtonPress = now;
  }

  // Nút SW4
  if (digitalRead(SW4) == LOW && now - lastButtonPress > debounceDelay) {
    showMessage("SW4 nhan");
    beep();
    lastButtonPress = now;
  }

  // Đọc RFID
  if (rdm6300.get_new_tag_id()) {
    currentTagID = String(rdm6300.get_tag_id(), HEX);
    Serial.print("RFID tag: ");
    Serial.println(currentTagID);
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
