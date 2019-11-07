/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2014-01-29
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_IO_CONTROL.java,v 1.4 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;

public class ADVABOARD_IO_CONTROL {
    public final static int CPLDINUPDATE_DISABLE = 0x0001;
    public final static int CPLDINUPDATE_ENABLE  = 0x0002;
    public final static int EVENT0_DISABLE       = 0x0100;
    public final static int EVENT0_ENABLE        = 0x0200;
    public final static int EVENT1_DISABLE       = 0x0400;
    public final static int EVENT1_ENABLE        = 0x0800;
    public final static int EVENT2_DISABLE       = 0x1000;
    public final static int EVENT2_ENABLE        = 0x2000;
    public final static int EVENT3_DISABLE       = 0x4000;
    public final static int EVENT3_ENABLE        = 0x8000;
}

