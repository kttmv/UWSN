import { SensorPlacementDistributionType } from '../tabs/SensorPlacement'

export function runSimulatorShell(args: string): Promise<void> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('run-simulator-close', () => {
            resolve()
        })

        window.electronAPI.ipcRenderer.send('run-simulator', args)
    })
}

export function runPlaceSensorsRandomStep(
    distributionType: SensorPlacementDistributionType,
    countX: number,
    countY: number,
    countZ: number,
    uniformA: number,
    uniformB: number,
    projectPath: string
): Promise<void> {
    const distr = SensorPlacementDistributionType[distributionType]
    let args = ''

    switch (Number(distributionType)) {
        case SensorPlacementDistributionType.Normal: {
            args +=
                `placeSensorsRndStep ${distr} ` +
                `${countX} ${countY} ${countZ} ` +
                `-f "${projectPath}"`
            break
        }
        case SensorPlacementDistributionType.Uniform: {
            args +=
                `placeSensorsRndStep ${distr} ` +
                `${countX} ${countY} ${countZ} ${uniformA} ${uniformB} 32 ` +
                `-f "${projectPath}"`
            break
        }
        default: {
            throw new Error('Что-то пошло не так')
        }
    }

    return runSimulatorShell(args)
}
