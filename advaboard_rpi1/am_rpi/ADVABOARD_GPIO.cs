/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2013-12-23
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_GPIO.java,v 1.2 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;

public class ADVABOARD_GPIO {
    public final static long I2C_INT  = 1 << 4;
    public final static long TP_INT   = 1 << 17;
    public final static long RPi_PWM  = 1 << 18;
    public final static long RPi_SEL0 = 1 << 27;
    public final static long RPi_SEL1 = 1 << 22;
    public final static long RPi_SEL2 = 1 << 23;
    public final static long RPi_SEL3 = 1 << 24;
    public final static long SPI_MISO = 1 << 9;
    public final static long LCD_RS   = 1 << 25;
}

