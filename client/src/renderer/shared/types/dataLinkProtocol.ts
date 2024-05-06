import { DataLinkProtocolType } from './dataLinkProtocolType'

export type PureAloha = {
    $type: DataLinkProtocolType.PureAloha
}

export type MultiChanneledAloha = {
    $type: DataLinkProtocolType.MultiChanneledAloha
}

export type DataLinkProtocol = PureAloha | MultiChanneledAloha
