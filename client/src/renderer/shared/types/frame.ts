import { Neighbour } from './neighbour'
import { Vector3 } from './vector3'

export type FrameType = 'RegularFrame' | 'Ack' | 'Data' | 'Hello'

export type Frame = {
    Type: FrameType
    SenderId: number
    SenderPosition: Vector3
    ReceiverId: number
    TimeSend: string
    AckIsNeeded: boolean
    NeighboursData: Neighbour[] | undefined
}
