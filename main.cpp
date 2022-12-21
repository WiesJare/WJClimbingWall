#include <Arduino.h>
//#include <SerialCommand.h>
#include <FastLED.h>
#include <iostream>
#include <string>

#define NUM_LEDS_1 64
#define NUM_LEDS_2 64
#define NUM_LEDS_3 64
#define NUM_LEDS_4 64
#define NUM_LEDS_5 64
#define NUM_LEDS_6 64
#define NUM_LEDS_7 64
#define NUM_LEDS_8 64

#define LED_PIN_1 15
#define LED_PIN_2 2
#define LED_PIN_3 4
#define LED_PIN_4 16
#define LED_PIN_5 17
#define LED_PIN_6 5
#define LED_PIN_7 18
#define LED_PIN_8 19

CRGB leds1[NUM_LEDS_1] = {0}; // Frame buffer for FastLED
/*CRGB leds2[NUM_LEDS_2] = {0}; // Frame buffer for FastLED
CRGB leds3[NUM_LEDS_3] = {0}; // Frame buffer for FastLED
CRGB leds4[NUM_LEDS_4] = {0}; // Frame buffer for FastLED
CRGB leds5[NUM_LEDS_5] = {0}; // Frame buffer for FastLED
CRGB leds6[NUM_LEDS_6] = {0}; // Frame buffer for FastLED
CRGB leds7[NUM_LEDS_7] = {0}; // Frame buffer for FastLED
CRGB leds8[NUM_LEDS_8] = {0}; // Frame buffer for FastLED
*/

byte data[256];

void setup() {

  FastLED.addLeds<WS2812B, LED_PIN_1, GRB>(leds1, NUM_LEDS_1);
  /*FastLED.addLeds<WS2812B, LED_PIN_2, GRB>(leds2, NUM_LEDS_2);
  FastLED.addLeds<WS2812B, LED_PIN_3, GRB>(leds3, NUM_LEDS_3);
  FastLED.addLeds<WS2812B, LED_PIN_4, GRB>(leds4, NUM_LEDS_4);
  FastLED.addLeds<WS2812B, LED_PIN_5, GRB>(leds5, NUM_LEDS_5);
  FastLED.addLeds<WS2812B, LED_PIN_6, GRB>(leds6, NUM_LEDS_6);
  FastLED.addLeds<WS2812B, LED_PIN_7, GRB>(leds7, NUM_LEDS_7);
  FastLED.addLeds<WS2812B, LED_PIN_8, GRB>(leds8, NUM_LEDS_8);
  */

  pinMode(LED_PIN_1, OUTPUT);
  /*pinMode(LED_PIN_2, OUTPUT);
  pinMode(LED_PIN_3, OUTPUT);
  pinMode(LED_PIN_4, OUTPUT);
  pinMode(LED_PIN_5, OUTPUT);
  pinMode(LED_PIN_6, OUTPUT);
  pinMode(LED_PIN_7, OUTPUT);
  pinMode(LED_PIN_8, OUTPUT);
  */

  Serial.begin(512000);
  Serial.setTimeout(1);
  //Serial.setRxFIFOFull(4);
}

void loop() {

  while (Serial.available() == 0) {} // Wait for serial

  int dataLength = 0;
  while (Serial.available() > 0) {
    data[dataLength] = Serial.read();
    dataLength++;
  }
  byte checksum1 = Serial.read();
  byte checksum2 = Serial.read();
  uint16_t receivedChecksum = (checksum2 << 8) | checksum1;

  uint16_t calculatedChecksum = 0;
  for (int i = 0; i < dataLength; i++) {
    calculatedChecksum += data[i];
  }   
  if (calculatedChecksum == receivedChecksum) {
    // The data is valid, so we can use it
    leds1[data[0]].setRGB(data[2], data[1], data[3]);
    leds1[calculatedChecksum].setRGB(data[2], data[1], data[3]);
    /*
    // Calculate pin
    int ledPin = (data[0] / 64) + 1;
    int pixelnum = data[0] % 64;

    // Set LED and light it up
    switch (ledPin) {
      case 1:
        leds1[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 2:
        leds2[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 3:
        leds3[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 4:
        leds4[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 5:
        leds5[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 6:
        leds6[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 7:
        leds7[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
      case 8:
        leds8[pixelnum].setRGB(data[2], data[1], data[3]);
        break;
    }*/
    FastLED.show();
  }
  else {
    leds1[calculatedChecksum].setRGB(255,0,0);
    leds1[receivedChecksum].setRGB(0,0,255);
    FastLED.show();
    //Mismatched checksum
  }

  // Delay for a few milliseconds
  //delay(1);
}