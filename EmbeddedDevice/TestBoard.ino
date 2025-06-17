#include <Wire.h>
#include <U8g2lib.h>
#include <Adafruit_NeoPixel.h>
#include <ArduinoJson.h>
#include <rdm6300.h>

U8G2_SSD1306_128X32_UNIVISION_F_SW_I2C u8g2(U8G2_R0, /* clock=*/ 22, /* data=*/ 21, /* reset=*/ U8X8_PIN_NONE);

#define SW1 23
#define SW2 25
#define SW3 26
#define SW4 27
#define SPEAKER 13
#define LED_PIN     4
#define NUM_LEDS    12

Adafruit_NeoPixel pixels(NUM_LEDS, LED_PIN, NEO_GRB + NEO_KHZ800);

int ledEffectMode = 2;
const int totalEffects = 6;
const float brightnessFactor = 0.2; // Giảm độ sáng xuống 20%

uint32_t ledColors[] = {
  pixels.Color(255, 0, 0),
  pixels.Color(0, 0, 255),
  pixels.Color(0, 255, 0),
  pixels.Color(255, 255, 0),
  pixels.Color(255, 165, 0),
  pixels.Color(128, 0, 128)
};
int colorCount = sizeof(ledColors) / sizeof(ledColors[0]);
int currentColorIndex = 0;

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
const unsigned long ledUpdateInterval = 100;
int ledStep = 0;
bool flashState = false;
float breathBrightness = 0;
bool breathIncreasing = true;

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
  rdm6300.begin(RDM6300_RX_PIN);
}

void loop() {
  if (Serial.available() > 0) {
    serialMessage = Serial.readStringUntil('\n');
    serialMessage.trim();
    if (serialMessage.length() > 0) {
      parseAndStoreJson(serialMessage);
      if (keyCount > 0) displayKeyValue();
    }
  }
  if (millis() - lastLedUpdate >= ledUpdateInterval) {
    switch (ledEffectMode) {
      case 0: pixels.clear(); pixels.show(); break;
      case 1: updateLedColor(ledColors[currentColorIndex]); break;
      case 2: runLedCircle(); break;
      case 3: rainbowEffect(); break;
      case 4: flashEffect(); break;
      case 5: breatheEffect(); break;
    }
    lastLedUpdate = millis();
  }
  unsigned long now = millis();
  if (digitalRead(SW1) == LOW && now - lastButtonPress > debounceDelay) {
    if (keyCount > 0) {
      currentIndex = (currentIndex - 1 + keyCount) % keyCount;
      displayKeyValue();
      beep();
    }
    lastButtonPress = now;
  }
  if (digitalRead(SW2) == LOW && now - lastButtonPress > debounceDelay) {
    if (keyCount > 0) {
      currentIndex = (currentIndex + 1) % keyCount;
      displayKeyValue();
      beep();
    }
    lastButtonPress = now;
  }
  if (digitalRead(SW3) == LOW && now - lastButtonPress > debounceDelay) {
    currentColorIndex = (currentColorIndex - 1 + colorCount) % colorCount;
    updateLedColor(ledColors[currentColorIndex]);
    beep();
    lastButtonPress = now;
  }
  if (digitalRead(SW4) == LOW && now - lastButtonPress > debounceDelay) {
    currentTagID = "";
    StaticJsonDocument<128> doc;
    doc["rfid"] = " ";
    String jsonStr;
    serializeJson(doc, jsonStr);
    Serial.println(jsonStr);
    displayKeyValue();
    beep();
    lastButtonPress = now;
  }
  if (rdm6300.get_new_tag_id()) {
    currentTagID = String(rdm6300.get_tag_id(), HEX);
    StaticJsonDocument<128> doc;
    doc["rfid"] = currentTagID;
    String jsonStr;
    serializeJson(doc, jsonStr);
    Serial.println(jsonStr);
    flashWithColor(pixels.Color(0, 255, 0));
    beep();
    displayKeyValue();
  }
  delay(50);
}

void beep() {
  digitalWrite(SPEAKER, HIGH);
  delay(100);
  digitalWrite(SPEAKER, LOW);
}

void parseAndStoreJson(String jsonStr) {
  DeserializationError error = deserializeJson(jsonDoc, jsonStr);
  if (error) return;
  keyCount = 0;
  currentIndex = 0;
  if (jsonDoc.containsKey("led_ring")) {
    String cmd = jsonDoc["led_ring"].as<String>();
    if (cmd == "ring_on") {
      ledEffectMode = (ledEffectMode + 1) % totalEffects;
      Serial.print("LED mode: ");
      Serial.println(ledEffectMode);
    }
  } else {
    for (JsonPair kv : jsonDoc.as<JsonObject>()) {
      if (keyCount < 10) {
        keys[keyCount] = kv.key().c_str();
        values[keyCount] = kv.value().as<String>();
        keyCount++;
      }
    }
  }
}

void displayKeyValue() {
  u8g2.clearBuffer();
  u8g2.drawStr(0, 10, "Serial JSON:");
  String displayStr = (keyCount > 0) ? keys[currentIndex] + ": " + values[currentIndex] : "";
  u8g2.drawStr(0, 20, displayStr.c_str());
  String rfidLine = "RFID: ";
  if (currentTagID.length() > 0) rfidLine += currentTagID;
  u8g2.drawStr(0, 30, rfidLine.c_str());
  u8g2.sendBuffer();
}

void updateLedColor(uint32_t color) {
  uint8_t r = (color >> 16) & 0xFF;
  uint8_t g = (color >> 8) & 0xFF;
  uint8_t b = color & 0xFF;
  r = r * brightnessFactor;
  g = g * brightnessFactor;
  b = b * brightnessFactor;
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, pixels.Color(r, g, b));
  }
  pixels.show();
}

void runLedCircle() {
  pixels.clear();
  int ledPos = ledStep % NUM_LEDS;
  pixels.setPixelColor(ledPos, dimColor(ledColors[currentColorIndex]));
  int prevLed = (ledPos - 1 + NUM_LEDS) % NUM_LEDS;
  pixels.setPixelColor(prevLed, dimColor(pixels.Color(0, 0, 50)));
  pixels.show();
  ledStep++;
}

void rainbowEffect() {
  for (int i = 0; i < NUM_LEDS; i++) {
    int hue = (ledStep * 10 + i * 360 / NUM_LEDS) % 360;
    pixels.setPixelColor(i, dimColor(HSBtoRGB(hue, 1.0, 0.5)));
  }
  pixels.show();
  ledStep++;
}

void flashEffect() {
  flashState = !flashState;
  uint32_t color = flashState ? pixels.Color(255, 255, 255) : pixels.Color(0, 0, 0);
  color = dimColor(color);
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, color);
  }
  pixels.show();
}

void breatheEffect() {
  float step = 0.02;
  if (breathIncreasing) {
    breathBrightness += step;
    if (breathBrightness >= 1.0) breathIncreasing = false;
  } else {
    breathBrightness -= step;
    if (breathBrightness <= 0.05) breathIncreasing = true;
  }
  uint8_t r = (uint8_t)(ledColors[currentColorIndex] >> 16 & 0xFF);
  uint8_t g = (uint8_t)(ledColors[currentColorIndex] >> 8 & 0xFF);
  uint8_t b = (uint8_t)(ledColors[currentColorIndex] & 0xFF);
  r *= breathBrightness * brightnessFactor;
  g *= breathBrightness * brightnessFactor;
  b *= breathBrightness * brightnessFactor;
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, pixels.Color(r, g, b));
  }
  pixels.show();
}

uint32_t HSBtoRGB(float hue, float sat, float brightness) {
  float r = 0, g = 0, b = 0;
  int i = int(hue / 60.0) % 6;
  float f = (hue / 60.0) - i;
  float p = brightness * (1.0 - sat);
  float q = brightness * (1.0 - f * sat);
  float t = brightness * (1.0 - (1.0 - f) * sat);
  switch (i) {
    case 0: r = brightness; g = t; b = p; break;
    case 1: r = q; g = brightness; b = p; break;
    case 2: r = p; g = brightness; b = t; break;
    case 3: r = p; g = q; b = brightness; break;
    case 4: r = t; g = p; b = brightness; break;
    case 5: r = brightness; g = p; b = q; break;
  }
  return pixels.Color(r * 255, g * 255, b * 255);
}

uint32_t dimColor(uint32_t color) {
  uint8_t r = (color >> 16) & 0xFF;
  uint8_t g = (color >> 8) & 0xFF;
  uint8_t b = color & 0xFF;
  r = r * brightnessFactor;
  g = g * brightnessFactor;
  b = b * brightnessFactor;
  return pixels.Color(r, g, b);
}

void flashWithColor(uint32_t color) {
  color = dimColor(color);
  for (int i = 0; i < NUM_LEDS; i++) {
    pixels.setPixelColor(i, color);
  }
  pixels.show();
  delay(300);
  pixels.clear();
  pixels.show();
}
