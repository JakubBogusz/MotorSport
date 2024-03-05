/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    domains: ["cdn.pixabay.com"],
  },
  output: "standalone",
  typescript: {
    ignoreBuildErrors: true,
  },
};

module.exports = nextConfig;
