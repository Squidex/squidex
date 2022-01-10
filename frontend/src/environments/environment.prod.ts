export const environment = {
    production: true,
    textLogger: false,
    textResolver: () => {
        return window['texts'];
    },
};
