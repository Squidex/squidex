function fixCoverage(contents) {
    this.cacheable();

    var ignores = [
        { name: 'arguments', line: 'var _a' },
        { name: 'decorate', line: 'var __decorate =', },
        { name: 'metadata', line: 'var __metadata =', },
        { name: 'extends', line: 'var __extends =', },
        { name: 'export', line: 'function __export' }
    ];

    var updates = 0;
    var rows = contents.split('\n');

    for (var rowIndex = 0; rowIndex < rows.length; rowIndex++) {
        var row = rows[rowIndex].trim();

        for (var ignoreIndex = 0; ignoreIndex < ignores.length; ignoreIndex++) {
            var ignore = ignores[ignoreIndex];

            if (row.indexOf(ignore.line) >= 0) {
                rows.splice(rowIndex, 0, '/* istanbul ignore next: TypeScript ' + ignore.name + ' */');
                rowIndex++;
                updates++;
                break;
            }
        }

        if (row.indexOf('hasOwnProperty') >= 0) {
            rows.splice(rowIndex, 0, '/* istanbul ignore else */');
            rowIndex++;
            updates++;
        }

        if (updates === ignores.length) {
            break;
        }
    }

    if (updates > 0) {
        return rows.join('\n');
    } else {
        return contents;
    }
}

module.exports = fixCoverage;