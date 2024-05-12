import { NetworkProtocolType } from './networkProtocolType'

export type BasicNetworkProtocol = {
    $type: NetworkProtocolType.BasicNetworkProtocol
    ResendWarningCount: number
}

export function isPureAloha(obj: any): obj is BasicNetworkProtocol {
    return obj.$type === NetworkProtocolType.BasicNetworkProtocol
}

export type NetworkProtocol = BasicNetworkProtocol // | ...
