import matplotlib.pyplot as plt
import numpy as np

# Data for the graph
groups = ["Control", "Experimental 1", "Experimental 2"]
means = [0.8005, 0.9428, 0.9408]
sds = [0.17550356, 0.03415590, 0.06314965]

# Create the bar plot
x = np.arange(len(groups))
plt.bar(x, means, yerr=sds, capsize=5, alpha=0.7, color=['blue', 'green', 'orange'])

# Add labels and title
plt.xticks(x, groups)
plt.ylabel("Jump Success Rate")
plt.title("Jump Success Rate by Group")

# Show the plot
plt.tight_layout()
plt.show()