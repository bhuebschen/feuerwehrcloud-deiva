#ifndef __WS2182WRAP_H__
#define __WS2182WRAP_H__

void Init(int LEDcount);

void setPixel(int pixel, unsigned char r, unsigned char g, unsigned char b);

void setPixelA(int pixel, unsigned char r, unsigned char g, unsigned char b, unsigned char a);

void updatePixels();

void Dispose();

#endif
