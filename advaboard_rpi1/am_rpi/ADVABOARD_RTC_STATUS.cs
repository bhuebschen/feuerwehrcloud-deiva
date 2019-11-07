/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2013-12-10
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_RTC_STATUS.java,v 1.3 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;

public class ADVABOARD_RTC_STATUS {
    public final static int RUN         = 0x02;
    public final static int ERROR       = 0x08;
    public final static int ALARM0_EN   = 0x10;
    public final static int ALARM0_FLAG = 0x20;
    public final static int ALARM1_EN   = 0x40;
    public final static int ALARM1_FLAG = 0x80;
}

