/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2013-12-10
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_RTC_CONTROL.java,v 1.3 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;

public class ADVABOARD_RTC_CONTROL {
    public final static int STOP       = 0x01;
    public final static int START      = 0x02;
    public final static int SET        = 0x04;
    public final static int CLRFLAGS   = 0x08;
    public final static int ALARM0_OFF = 0x10;
    public final static int ALARM0_ON  = 0x20;
    public final static int ALARM1_OFF = 0x40;
    public final static int ALARM1_ON  = 0x80;
}

