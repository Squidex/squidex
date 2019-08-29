import { exec } from 'child_process';
import waitOn from 'wait-on';

function runCommand(text: string) {
    return new Promise((resolve, reject) => {
        const command = exec(text);

        command.stdout.on('data', (data) => {
            console.log(data.toString());
        });

        command.stderr.on('data', (data) => {
            console.error(data.toString());
        });

        command.on('exit', (code: number) => {
            if (code === 0) {
                resolve(code);
            } else {
                reject(`Command failed with exit code: ${code}`);
            }
        });
    });
}

export async function runDeployment(baseUrl: string) {
    console.log('[Deployment] Waiting for application to start');

    await waitOn({
        resources: [
            baseUrl
        ],
        strictSSL: false
    });

    console.log('[Deployment] Starting');

    await runCommand(`cd ../tools/DeploymentApp/DeploymentApp && dotnet run --url ${baseUrl} --skip-rules`);

    console.log('[Deployment] Completed');
}