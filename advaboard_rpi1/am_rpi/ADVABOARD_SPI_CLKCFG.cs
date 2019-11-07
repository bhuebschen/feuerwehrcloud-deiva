/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2013-12-23
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_SPI_CLKCFG.java,v 1.2 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;

public class ADVABOARD_SPI_CLKCFG {
    public final static int IDLELOW_SHIFTFALLING_READRISING  = 0;
    public final static int IDLELOW_SHIFTRISING_READFALLING  = 1;
    public final static int IDLEHIGH_SHIFTRISING_READFALLING = 2;
    public final static int IDLEHIGH_SHIFTFALLING_READRISING = 3;
}


