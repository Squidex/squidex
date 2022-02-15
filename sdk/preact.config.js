export default (config, env, helpers, options) => {
	config.output.filename = (pathData) => {
		if (pathData.chunk.name === 'polyfills') {
			return 'polyfill.js';
		} else {
			return 'embed-sdk.js';
		}
	};

	config.plugins[4].options.filename = 'embed-sdk.css'
};