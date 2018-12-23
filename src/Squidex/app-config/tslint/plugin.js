const path = require('path');
const chalk = require('chalk');
const { fork } = require('child_process');

function apply(options, compiler) {
  let linterProcess;
  let linterPromise;
  let linterIteration = 0;

  function compileHook() {
    if (linterProcess && linterProcess.kill) {
      // Exits any outstanding child process if one exists
      linterProcess.kill();
    }

    let { files = [] } = options;

    if (!files.length) {
      process.stdout.write(chalk.yellow.bold('\n[tslint-plugin] No `files` option specified.\n'));
      return;
    }

    options.files = Array.isArray(files) ? files : [files];

    // Spawn a child process to run the linter
    linterProcess = fork(path.resolve(__dirname, 'linter.js'), [JSON.stringify(options)], {
      silent: true
    });

    // Use the iteration to cancel previous promises.
    linterIteration++;

    linterPromise = new Promise(resolve => {
      const linterOutBuffer = [];

      linterProcess.stdout.on('data', (message) => {
        if (message) {
          const msg = message.toString();
  
          for (let line of msg.split('\n')) {
            const indexOfSeparator = line.indexOf(':');
  
            if (indexOfSeparator > 0) {
              const type = line.substring(0, indexOfSeparator);
              const body = line.substring(indexOfSeparator + 1);
  
              switch (type) {
                case 'tslint': {
                  const json = JSON.parse(body);
  
                  for (let item of json) {
                    linterOutBuffer.push(item);
                  }
  
                  break;
                }
                case 'tsinfo': {
                  process.stdout.write(chalk.cyan(`[tslint-plugin] ${body}\n`));
                  break;
                }
                case 'tserror': {
                  process.stderr.write(chalk.red(`[tslint-plugin] ${body}\n`));
                  break;
                }
                default: {
                  process.stderr.write(msg);
                }
              }
            } else {
              process.stdout.write(line);
            }
          }
        }
      });

      linterProcess.once('exit', () => {
        resolve({ iteration: linterIteration, out: linterOutBuffer });
  
        // Clean up the linterProcess when finished
        delete linterProcess;
      });
    });
  }

  function createError(message) {
    const error = new Error(message);
    delete error.stackTrace;
    return error;
  }

  function emitHook(compilation, callback) {
    if (linterPromise && options.waitForLinting) {
        linterPromise.then(result => {
        for (let r of result.out) {
          const msg = `${r.name}:${r.startPosition.line + 1}:${r.startPosition.character + 1} [tslint] ${r.ruleName}: ${r.failure}`;

          if (r.ruleSeverity === 'ERROR' || options.warningsAsError) {
            compilation.errors.push(createError(msg));
          } else {
            compilation.warnings.push(createError(msg));
          }
        }

        callback();
      });
    } else {
      callback();
    }
  }

  function doneHook() {
    const currentIteration = linterIteration;
    
    if (linterPromise && !options.waitForLinting) {
      let isResolved = false;

      linterPromise.then(result => {
        isResolved = true;

        // If the iterations are not the same another process has already been started and we cancel these results.
        if (result.iteration === currentIteration) {
          for (let r of result.out) {
            const msg = `${r.name}:${r.startPosition.line + 1}:${r.startPosition.character + 1} [tslint] ${r.ruleName}: ${r.failure}`;

            if (r.ruleSeverity === 'ERROR' || options.warningsAsError) {
              process.stderr.write(chalk.red(msg + '\n'));
            } else {
              process.stdout.write(chalk.yellow(msg + '\n'));
            }
          }
        }
      });

      if (!isResolved) {
        process.stdout.write(chalk.cyan(`[tslint-plugin] Waiting for results...\n`));
      }
    }
  }

  if (compiler.hooks) {
    // Webpack 4
    compiler.hooks.compile.tap('TSLintWebpackPlugin', compileHook);
    compiler.hooks.emit.tapAsync('TSLintWebpackPlugin', emitHook);
    compiler.hooks.done.tap('TSLintWebpackPlugin', doneHook);
  } else {
    // Backwards compatibility
    compiler.plugin('compile', compileHook);
    compiler.plugin('emit', emitHook);
    compiler.plugin('done', doneHook);
  }
}

module.exports = function TSLintWebpackPlugin(options = {}) {
  return {
    apply: apply.bind(this, options)
  };
};
