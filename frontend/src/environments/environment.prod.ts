export const environment = {
    production: true,
    textLogger: false,
    textResolver: () => {
        return (window as any)['texts'];
    },
};
