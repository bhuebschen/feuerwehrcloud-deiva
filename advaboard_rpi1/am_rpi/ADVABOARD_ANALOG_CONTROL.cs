/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2013-12-10
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_ANALOG_CONTROL.java,v 1.3 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;

public class ADVABOARD_ANALOG_CONTROL {
    public final static long ADC0_STOP       = 0x01;
    public final static long ADC0_ONESHOT    = 0x02;
    public final static long ADC0_CONTINUOUS = 0x03;
    public final static long ADC1_STOP       = 0x04;
    public final static long ADC1_ONESHOT    = 0x08;
    public final static long ADC1_CONTINUOUS = 0x0C;
    public final static long ADC2_STOP       = 0x10;
    public final static long ADC2_ONESHOT    = 0x20;
    public final static long ADC2_CONTINUOUS = 0x30;
    public final static long ADC3_STOP       = 0x40;
    public final static long ADC3_ONESHOT    = 0x80;
    public final static long ADC3_CONTINUOUS = 0xC0;
    public final static long ADC4_STOP       = 0x0100;
    public final static long ADC4_ONESHOT    = 0x0200;
    public final static long ADC4_CONTINUOUS = 0x0300;
    public final static long IDA0_DISABLE    = 0x1000;
    public final static long IDA0_ENABLE     = 0x2000;
    public final static long IDA1_DISABLE    = 0x4000;
    public final static long IDA1_ENABLE     = 0x8000;
    public final static long PWM0_DISABLE    = 0x010000;
    public final static long PWM0_PWM8       = 0x020000;
    public final static long PWM0_PWM16      = 0x030000;
    public final static long PWM0_FREQ       = 0x040000;
    public final static long PWM1_DISABLE    = 0x01000000;
    public final static long PWM1_PWM8       = 0x02000000;
    public final static long PWM1_PWM16      = 0x03000000;
    public final static long PWM1_FREQ       = 0x04000000;
}


