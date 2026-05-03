export default {
  defaultIgnores: true,
  parserPreset: {
    parserOpts: {
      headerPattern: /^(RPS-\d+)\s([a-z]+)(?:\(([a-z0-9-]+)\))?(!?):\s(.+)$/,
      headerCorrespondence: ["ticket", "type", "scope", "breaking", "subject"]
    }
  },
  ignores: [
    (message) => /^chore(?:\([^)]+\))?: release\b/i.test(message.trim())
  ],
  rules: {
    "type-case": [2, "always", "lower-case"],
    "type-empty": [2, "never"],
    "subject-empty": [2, "never"],
    "type-enum": [2, "always", ["feat", "fix", "docs", "test", "refactor", "chore"]]
  }
};
