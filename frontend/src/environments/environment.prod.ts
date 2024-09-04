export const environment = {
    production: true,
    textResolver: () => {
        return (window as any)['texts'];
    },
};
