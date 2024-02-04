import { Vector3 } from 'three'
import { runSimulatorShell } from '..'

export function runInitialization(vec1: Vector3, vec2: Vector3) {
    runSimulatorShell(
        `init ${vec1.x} ${vec1.y} ${vec1.z} ${vec2.x} ${vec2.y} ${vec2.z} --file D:\\Env.json`
    )
}
