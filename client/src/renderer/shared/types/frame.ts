import { CollectedData } from './collectedData'
import { Vector3 } from './vector3'

export enum FrameType {
    RegularFrame = 'RegularFrame',
    Ack = 'Ack',
    Data = 'Data',
    Hello = 'Hello',
    Warning = 'Warning'
}

export type Frame = {
    Type: FrameType
    SenderId: number
    SenderPosition: Vector3
    ReceiverId: number
    TimeSend: string
    AckIsNeeded: boolean
    CollectedData: CollectedData | undefined
    //NeighboursData: Neighbour[] | undefined
    //DeadSensors: number[] | undefined
}
