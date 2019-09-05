import { ChildProcess, exec } from 'child_process';

let squidex: ChildProcess;

export function startSquidex(silent = true) {
    console.log('[Squidex] Starting');

    squidex = exec('cd ../src/squidex && dotnet run');

    squidex.stdout.on('data', (data) => {
        const text = data.toString();

        if (!silent || text.indexOf('exception') >= 0) {
            console.log(text);
        }
    });

    squidex.stderr.on('data', (data) => {
        const text = data.toString();

        if (!silent || text.indexOf('exception') >= 0) {
            console.error(text);
        }
    });

    squidex.on('close', (code) => {
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
        console.log('[Squidex] Stopping');

        squidex.kill('SIGKILL');

        console.log('[Squidex] Stopped');
    }
}