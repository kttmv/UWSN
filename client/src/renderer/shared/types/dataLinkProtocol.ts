import { DataLinkProtocolType } from './dataLinkProtocolType'

export type PureAloha = {
    $type: DataLinkProtocolType.PureAloha
    Timeout: number
    TimeoutRelativeDeviation: number
    AckTimeout: number
}

export function isPureAloha(obj: any): obj is PureAloha {
    return obj.$type === DataLinkProtocolType.PureAloha
}

export type MultiChanneledAloha = {
    $type: DataLinkProtocolType.MultiChanneledAloha
    Timeout: number
    TimeoutRelativeDeviation: number
    AckTimeout: number
}

export function isMultichanneledAloha(obj: any): obj is MultiChanneledAloha {
    return obj.$type === DataLinkProtocolType.MultiChanneledAloha
}

export type DataLinkProtocol = PureAloha | MultiChanneledAloha
