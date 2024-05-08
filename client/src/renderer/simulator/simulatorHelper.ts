import { RandomStepDistributionType } from '../tabs/SensorPlacement'

export function runSimulatorShell(args: string): Promise<void> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('run-shell-close', () => {
            resolve()
        })

        window.electronAPI.ipcRenderer.send('run-shell', args)
    })
}

export function runSimulatorShellNoStdout(args: string): Promise<void> {
    return new Promise((resolve) => {
        window.electronAPI.ipcRenderer.once('run-shell-close', () => {
            resolve()
        })

        window.electronAPI.ipcRenderer.send('run-shell-simulation', args)
    })
}

export function runSimulation(projectPath: string): Promise<void> {
    return runSimulatorShellNoStdout(`runSim -f "${projectPath}"`)
}

export function runPlaceSensorsRandomStep(
    distributionType: RandomStepDistributionType,
    countX: number,
    countY: number,
    countZ: number,
    uniformA: number,
    uniformB: number,
    projectPath: string
): Promise<void> {
    const distr = RandomStepDistributionType[distributionType]
    let args = ''

    switch (Number(distributionType)) {
        case RandomStepDistributionType.Normal: {
            args +=
                `placeSensorsRndStep ${distr} ` +
                `${countX} ${countY} ${countZ} ` +
                `-f "${projectPath}"`
            break
        }
        case RandomStepDistributionType.Uniform: {
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
