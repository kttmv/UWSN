export enum ModemType {
    AquaModem1000 = 'UWSN.Model.Modems.AquaModem1000, UWSN',
    AquaModem500 = 'UWSN.Model.Modems.AquaModem500, UWSN',
    AquaCommMako = 'UWSN.Model.Modems.AquaCommMako, UWSN',
    AquaCommMarlin = 'UWSN.Model.Modems.AquaCommMarlin, UWSN',
    AquaCommOrca = 'UWSN.Model.Modems.AquaCommOrca, UWSN',
    MicronModem = 'UWSN.Model.Modems.MicronModem, UWSN',
    SMTUTestModem = 'UWSN.Model.Modems.SMTUTestModem, UWSN'
}

export type Modem = {
    $type: ModemType
}
