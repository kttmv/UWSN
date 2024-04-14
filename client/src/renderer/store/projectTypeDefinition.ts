enum NetworkProtocolType {
    PureAloha = 'UWSN.Model.Protocols.Network.PureAlohaProtocol, UWSN, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
}

type Vector3Data = {
    X: number
    Y: number
    Z: number
}

type EnvironmentData = {
    Sensors: SensorData[]
}

type SensorData = {
    Id: number
    Position: Vector3Data
}

export type ProjectData =
    | undefined
    | {
          NetworkProtocolType: NetworkProtocolType
          AreaLimits: {
              Min: Vector3Data
              Max: Vector3Data
          }
          ChannelManager: {
              NumberOfChannels: number
          }
          Environment: {
              Sensors: SensorData[]
          }
      }
