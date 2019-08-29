import { ChildProcess, exec } from 'child_process';

let squidex: ChildProcess;

export function startSquidex() {
    console.log('[Squidex] Starting');

    squidex = exec('cd ../src/squidex && dotnet run');

    squidex.stdout.on('data', (data) => {
        console.log(data.toString());
    });

    squidex.stderr.on('data', (data) => {
        console.error(data.toString());
    });

    squidex.on('exit', (code) => {
        console.log(`Child exited with code ${code}`);

        if (code !== 0) {
            console.log('[Squidex] Failed to start');

            process.exit(code);
        }
    });

    console.log('[Squidex] Started');
}

export function stopSquidex() {
    if (squidex) {
        squidex.kill();

        console.log('[Squidex] Stopped');
    }
}